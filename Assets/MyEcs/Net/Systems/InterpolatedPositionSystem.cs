using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Mirror;


namespace MyEcs.Net
{
    public class InterpolatedPositionSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        public static int snapshotMaxCount = 5;
        public static float interpolationDelay = 0.1f;



        readonly EcsPoolInject<ECInterpolatePositionReceive> recPool = default;

        readonly EcsFilterInject<Inc<ECInterpolatePositionReceive, ECPosition>> recFilter = default;
        readonly EcsFilterInject<Inc<ECInterpolatePositionSend, ECPosition, ECNetId>> sendFilter = default;

        InterpolatePositionMessage bufferMessage;
        public void Init(IEcsSystems systems)
        {
            bufferMessage = new InterpolatePositionMessage();
            if (NetStatic.IsServer)
                NetworkServer.RegisterHandler<InterpolatePositionMessage>(ReadSyncPacketServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<InterpolatePositionMessage>(ReadSyncPacketClient);
            else
                throw new Exception(typeof(SyncReceiveSystem).Name + " system started with no network setup");
        }
        public void Run(IEcsSystems systems)
        {
            foreach (int i in recFilter.Value)
                UpdateRecComponent(ref recFilter.Pools.Inc1.Get(i), ref recFilter.Pools.Inc2.Get(i));
            foreach (int i in sendFilter.Value)
                UpdateSendComponent(ref sendFilter.Pools.Inc1.Get(i), ref sendFilter.Pools.Inc2.Get(i), ref sendFilter.Pools.Inc3.Get(i));
        }

        [Server]
        public void ReadSyncPacketServer(NetworkConnection conn, InterpolatePositionMessage msg)
        {
            ReadSyncPacket(ref msg, (float)conn.remoteTimeStamp);
        }
        [Client]
        public void ReadSyncPacketClient(InterpolatePositionMessage msg)
        {
            ReadSyncPacket(ref msg, (float)NetworkClient.connection.remoteTimeStamp);
        }
        public void ReadSyncPacket(ref InterpolatePositionMessage msg, float time)
        {
            if (!NetIdService.TryGetEntity(msg.netId, out int ent))
                return;
            if (!recPool.Value.Has(ent))
                return;
            ref var syncState = ref recPool.Value.Get(ent);
            syncState.AddSnapshot(msg.position, time);
        }
        void UpdateRecComponent(ref ECInterpolatePositionReceive inter, ref ECPosition pos)
        {
            if (!inter.HasAny())
                return;
            pos.position = inter.GetPosition((float)NetworkTime.time - interpolationDelay);
        }
        void UpdateSendComponent(ref ECInterpolatePositionSend send, ref ECPosition pos, ref ECNetId netId)
        {
            if (send.nextSend > Time.time)
                return;
            send.nextSend = Time.time + send.sendPeriod;
            bufferMessage.netId = netId.id;
            bufferMessage.position = pos.position;
            if (NetStatic.IsServer)
                NetCommunication.BroadcastToReadyExcept(send.exceptionConnectionId, bufferMessage, Channels.Unreliable);
            else
                NetCommunication.SendToServer(bufferMessage, Channels.Unreliable);
        }
        public void Destroy(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                NetworkServer.UnregisterHandler<InterpolatePositionMessage>();
            else if (NetStatic.IsClient)
                NetworkClient.UnregisterHandler<InterpolatePositionMessage>();
        }
    }
    public struct ECInterpolatePositionSend
    {
        public int exceptionConnectionId;
        public float nextSend;
        public float sendPeriod;
    }
    public struct ECInterpolatePositionReceive : IEcsAutoReset<ECInterpolatePositionReceive>
    {
        Snapshot[] snapshots;
        int totalAdded;
        int _ringPointer;
        int lastAddedPointer
        {
            get => _ringPointer;
            set => _ringPointer = GetRingIndex(value);
        }
        struct Snapshot
        {
            public Snapshot(Vector2 pos, float time)
            {
                position = pos;
                timeStamp = time;
            }
            public Vector2 position;
            public float timeStamp;
        }

        int GetRingIndex(int num) => num % InterpolatedPositionSystem.snapshotMaxCount; //works incorrectly for negatives
        public bool HasAny() => totalAdded > 0;
        public void Reset(Vector2 position, float time)
        {
            var snap = new Snapshot(position, time);
            for (int i = 0; i < InterpolatedPositionSystem.snapshotMaxCount; i++)
                snapshots[i] = snap;
            totalAdded = 0;
            _ringPointer = 0;
        }
        public void AddSnapshot(Vector2 position, float time)
        {
            if (time < snapshots[lastAddedPointer].timeStamp)
                return;
            lastAddedPointer = totalAdded;
            totalAdded++;
            snapshots[lastAddedPointer] = new Snapshot(position, time);
        }
        public Vector2 GetPosition(float time)
        {
            var nextSnap = snapshots[lastAddedPointer];
            if (nextSnap.timeStamp <= time)
                return nextSnap.position;
            for (int i = 1; i < InterpolatedPositionSystem.snapshotMaxCount; i++)
            {
                var prevSnap = snapshots[GetRingIndex(lastAddedPointer - i + InterpolatedPositionSystem.snapshotMaxCount)];
                if (prevSnap.timeStamp <= time)
                {
                    float mod = (time - prevSnap.timeStamp) / (nextSnap.timeStamp - prevSnap.timeStamp);
                    return Vector2.Lerp(prevSnap.position, nextSnap.position, mod);
                }
                nextSnap = prevSnap;
            }
            return nextSnap.position;
        }
        public void AutoReset(ref ECInterpolatePositionReceive c)
        {
            if (c.snapshots == null)
                c.snapshots = new Snapshot[InterpolatedPositionSystem.snapshotMaxCount];
            else
                c.snapshots[0] = new Snapshot();
            c.totalAdded = 0;
            c._ringPointer = 0;
        }
    }
    public struct InterpolatePositionMessage : NetworkMessage
    {
        public ushort netId;
        public Vector2 position;
    }
}