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
    public class SyncReceiveSystem : IEcsInitSystem, IEcsDestroySystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsPoolInject<ECSyncReceive> recPool = default;

        public void Init(IEcsSystems systems)
        {

            if (NetStatic.IsServer)
                NetworkServer.RegisterHandler<NetSyncMessage>(ReadSyncPacketServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<NetSyncMessage>(ReadSyncPacketClient);
            else
                throw new Exception(typeof(SyncReceiveSystem).Name + " system started with no network setup");
        }

        [Server]
        public void ReadSyncPacketServer(NetworkConnection conn, NetSyncMessage msg)
        {
            if (!conn.OwnsNetId(msg.netId))
                return;
            ReadSyncPacket(ref msg, (float)conn.remoteTimeStamp);
        }
        [Client]
        public void ReadSyncPacketClient(NetSyncMessage msg)
        {
            ReadSyncPacket(ref msg, (float)NetworkClient.connection.remoteTimeStamp);
        }
        public void ReadSyncPacket(ref NetSyncMessage msg, float time)
        {
            if (!NetIdService.TryGetEntity(msg.netId, out int ent))
                return;
            if (!recPool.Value.Has(ent))
                return;
            ref var syncState = ref recPool.Value.Get(ent);
            if (syncState.lastTime > time)
                return;
            syncState.lastTime = time;

            msg.payload.UnloadToWorld(world.Value, ent);
        }

        public void Destroy(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                NetworkServer.UnregisterHandler<NetSyncMessage>();
            else if (NetStatic.IsClient)
                NetworkClient.UnregisterHandler<NetSyncMessage>();
        }
    }
    public struct ECSyncReceive
    {
        public float lastTime;
    }
}