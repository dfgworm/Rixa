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
    public class NetSpawnSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsPoolInject<ECNetAutoSpawn> autoSpawnPool = default;

        readonly EcsFilterInject<Inc<ECNetAutoSpawn>> autoSpawnFilter = default;
        readonly EcsFilterInject<Inc<EMSpawned, ECNetAutoSpawn>> justSpawnedFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;

        NetSpawnMessage bufferMessage;
        public static NetSpawnSystem _instance;
        [Server]
        public static void SendAllToClient(NetworkConnectionToClient conn)
        {
            foreach (int i in _instance.autoSpawnFilter.Value)
                NetCommunication.SendToClient(conn, _instance.ConstructMessage(i), Channels.Reliable);
        }
        public void Init(IEcsSystems systems)
        {
            _instance = this;
            if (NetStatic.IsServer) { }
            //NetworkServer.RegisterHandler<NetSpawnMessage>(ReadSyncPacketServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<NetSpawnMessage>(ReadPacketClient);
        }

        public void Run(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                foreach (int i in justSpawnedFilter.Value)
                {
                    justSpawnedFilter.Pools.Inc2.Get(i).payload = justSpawnedFilter.Pools.Inc1.Get(i).payload;
                    var msg = _instance.ConstructMessage(i);
                    NetCommunication.BroadcastToReady(msg, Channels.Reliable);
                }
        }
        [Client]
        public void ReadPacketClient(NetSpawnMessage msg)
        {
            ReadPacket(ref msg, (float)NetworkClient.connection.remoteTimeStamp);
        }
        public void ReadPacket(ref NetSpawnMessage msg, float time)
        {
            ref var spawnEv = ref bus.Value.NewEvent<EVSpawn>();
            spawnEv.payload = msg.payload;
        }

        public void Destroy(IEcsSystems systems)
        {
            if (_instance == this)
                _instance = null;
            bufferMessage.payload = null;
            if (NetStatic.IsServer) { }
            //NetworkServer.UnregisterHandler<NetSpawnMessage>();
            else if (NetStatic.IsClient) { }
                NetworkClient.UnregisterHandler<NetSpawnMessage>();
        }

        public NetSpawnMessage ConstructMessage(int entity)
        {
            bufferMessage.payload = autoSpawnPool.Value.Get(entity).payload;
            bufferMessage.payload.UpdateThis(world.Value, entity);
            return bufferMessage;
        }
    }
    public struct ECNetAutoSpawn
    {
        public BaggagePayload payload;
    }

    public struct NetSpawnMessage :NetworkMessage
    {
        public BaggagePayload payload;
    }
}