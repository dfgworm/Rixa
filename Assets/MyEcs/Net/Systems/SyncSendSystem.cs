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
            bufferPayload.List.Clear();
            int c = send.payload.GetNeedsUpdate(world.Value, ent, bufferPayload.List);
            if (c > 0)
            {
                bufferMessage.netId = netId.id;
                bufferPayload.UpdateThis(world.Value, ent);
                if (NetStatic.IsServer)
                    NetCommunication.BroadcastToReadyExcept(send.exceptionConnectionId, bufferMessage, Channels.Unreliable);
                else
                    NetCommunication.SendToServer(bufferMessage, Channels.Unreliable);
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
        public int exceptionConnectionId;
        public BaggagePayload payload;
    }
    public struct NetSyncMessage :NetworkMessage
    {
        public ushort netId;
        public BaggagePayload payload;
    }
}