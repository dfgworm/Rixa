using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Mirror;

using MyEcs.Spawn;

namespace MyEcs.Net
{
    public class SyncSendSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsFilterInject<Inc<ECSyncSend, ECNetId>, Exc<EMSpawned>> syncFilter = default;

        NetSyncMessage bufferMessage;
        BaggagePayload bufferPayload;
        public void Init(IEcsSystems systems)
        {
            bufferPayload = new BaggagePayload ();
            bufferMessage = new NetSyncMessage { payload = bufferPayload };
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in syncFilter.Value)
                SendSync(i, ref syncFilter.Pools.Inc1.Get(i), ref syncFilter.Pools.Inc2.Get(i));
        }
        public void SendSync(int ent, ref ECSyncSend send, ref ECNetId netId)
        {
            if (!send.IsReadyToSend())
                return;
            send.SetNext();
            bufferPayload.List.Clear();
            int c = send.Payload.GetNeedsUpdate(world.Value, ent, bufferPayload.List);
            if (c > 0)
            {
                bufferMessage.netId = netId.id;
                bufferPayload.UpdateThis(world.Value, ent);
                if (NetStatic.IsServer) //you can find exception by getting the owner of entity, no need for exceptionId
                    NetCommunication.BroadcastToReadyExcept(send.exceptionConnectionId, bufferMessage, Channels.Reliable);
                else
                    NetCommunication.SendToServer(bufferMessage, Channels.Reliable);
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            bufferPayload = null;
            bufferMessage.payload = null;
        }
    }
    public struct ECSyncSend
    {
        public float sendPeriod;
        public int exceptionConnectionId;
        float nextSend;
        BaggagePayload payload;
        public BaggagePayload Payload
        {
            get {
                if (payload == null)
                    payload = new BaggagePayload();
                return payload;
            }
        }
        public bool IsReadyToSend() => nextSend < Time.time;
        public void SetNext() => nextSend = Time.time + sendPeriod;
    }
    public struct NetSyncMessage :NetworkMessage
    {
        public ushort netId;
        public BaggagePayload payload;
    }
}