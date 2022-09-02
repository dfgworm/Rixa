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
    public class NetDestroySystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<ECNetId, ECDestroy>> destroyFilter = default;

        HashSet<DelayedDestroy> delayed;
        NetDestroyMessage bufferMessage;
        public void Init(IEcsSystems systems)
        {
            delayed = new HashSet<DelayedDestroy>(10);
            bufferMessage = new NetDestroyMessage();
            if (NetStatic.IsServer) { }
            //NetworkServer.RegisterHandler<NetSpawnMessage>(ReadSyncPacketServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<NetDestroyMessage>(ReadPacketClient);
        }

        public void Run(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                foreach (int i in destroyFilter.Value)
                    RelayDestroy(ref destroyFilter.Pools.Inc1.Get(i));
            if (NetStatic.IsClient)
                ProcessDelayed();
        }
        void ProcessDelayed()
        {
            delayed.RemoveWhere(x => x.TryExecute() || x.HasExpired);
        }
        [Client]
        public void ReadPacketClient(NetDestroyMessage msg)
        {
            ReadDestroyPacket(msg);
        }
        public void ReadDestroyPacket(NetDestroyMessage msg)
        {
            if (!NetIdService.TryGetEntity(msg.netId, out int ent))
            {
                delayed.Add(new DelayedDestroy(msg.netId));
                return;
            }
            world.Value.GetPool<ECDestroy>().SoftAdd(ent);
        }

        void RelayDestroy(ref ECNetId netId)
        {
            bufferMessage.netId = netId.id;
            NetCommunication.BroadcastToReady(bufferMessage, Channels.Reliable);
        }

        public void Destroy(IEcsSystems systems)
        {
            if (NetStatic.IsServer) { }
            //NetworkServer.UnregisterHandler<NetSpawnMessage>();
            else if (NetStatic.IsClient) { }
                NetworkClient.UnregisterHandler<NetDestroyMessage>();
        }
        struct DelayedDestroy
        {
            public bool HasExpired => expireTime < NetworkTime.time && expireFrame < Time.frameCount;
            ushort netId;
            int expireFrame;
            double expireTime;
            public DelayedDestroy(ushort _netId)
            {
                netId = _netId;
                expireFrame = Time.frameCount + 100;
                expireTime = NetworkTime.time + 2;
            }
            public bool TryExecute()
            {
                if (NetIdService.TryGetEntity(netId, out int ent))
                {
                    EcsStatic.GetPool<ECDestroy>().SoftAdd(ent);
                    return true;
                }
                return false;
            }
            public override bool Equals(object ob)
            {
                if (ob is DelayedDestroy)
                    return ((DelayedDestroy)ob).netId == netId;
                else
                    return false;
            }
            public override int GetHashCode()
            {
                return netId;
            }
        }
    }

    public struct NetDestroyMessage :NetworkMessage
    {
        public ushort netId;
    }
}