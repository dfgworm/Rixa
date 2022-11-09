using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Mirror;
using MyEcs.Spawn;
using MyEcs.Net;
using MyEcs.Health;

namespace MyEcs.Actions
{
    public class ActionSystem : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsPoolInject<ACProjectileLaunch> projLaunchPool = "actions";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;

        NetActionMessage bufferMessage;
        public void Init(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                NetworkServer.RegisterHandler<NetActionMessage>(ReadMsgServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<NetActionMessage>(ReadmsgClient);
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVActionUse>(out var pool))
                TrySendUseEvent(ref pool.Get(i));
            foreach (int i in bus.Value.GetEventBodies<AEVActionUse>(out var pool))
                ProcessActionUse(ref pool.Get(i));

        }
        [Server]
        public void ReadMsgServer(NetworkConnection conn, NetActionMessage msg)
        {
            if (!conn.OwnsNetId(msg.entityId))
                return;
            NetCommunication.BroadcastToReadyExcept(conn.connectionId, msg, Channels.Reliable);
            ReadMsg(ref msg, (float)conn.remoteTimeStamp);
        }
        [Client]
        public void ReadmsgClient(NetActionMessage msg)
        {
            ReadMsg(ref msg, (float)NetworkClient.connection.remoteTimeStamp);
        }
        public void ReadMsg(ref NetActionMessage msg, float time)
        {
            if (!NetIdService.TryGetEntity(msg.entityId, out int ent))
                return;
            if (!EcsActionService.TryGetAction(ent, msg.actionId, out int ac))
                return;
            ref var ev = ref bus.Value.NewEvent<AEVActionUse>();
            ev.netReceived = true;
            ev.action = acWorld.Value.PackEntity(ac);
            ev.target = msg.target;
        }
        public void TrySendUseEvent(ref AEVActionUse useEvent)
        {
            if (useEvent.netReceived || !useEvent.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!EcsActionService.TryGetEntity(ac, out int ent) || !world.Value.GetPool<ECNetId>().Has(ent))
                return;
            if (!PlayerBehaviourBase.IsEntityLocalAuthority(ent))
                return;
            ref var netId = ref world.Value.GetPool<ECNetId>().Get(ent);

            bufferMessage.entityId = netId.id;
            bufferMessage.actionId = EcsActionService.GetActionId(ac);
            bufferMessage.target = useEvent.target;
            if (NetStatic.IsServer)
                NetCommunication.BroadcastToReady(bufferMessage, Channels.Reliable);
            else
                NetCommunication.SendToServer(bufferMessage, Channels.Reliable);
        }

        void ProcessActionUse(ref AEVActionUse ev)
        {
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!projLaunchPool.Value.Has(ac))
                return;
            if (!acWorld.Value.GetPool<ACEntity>().Get(ac).entity.Unpack(world.Value, out int ent))
                return;

            Vector2 pos = world.Value.GetPool<ECPosition>().Get(ent).position;
            Vector2 target = ev.target;
            // launching most projectiles is better done through direction, for better net sync purposes

            ref var proj = ref projLaunchPool.Value.Get(ac);
            ref var spawnEv = ref bus.Value.NewEvent<EVSpawn>();
            spawnEv.Payload
                .Add(SpawnPipelineBaggage.Get<ProjectileSP>())
                .Add(new PositionBaggage { position = pos })
                .Add(new VelocityBaggage { velocity = (target - pos).normalized * proj.velocity })
                .Add(new ProjectileBaggage {
                    selfDestruct = proj.selfDestruct,
                    damage = proj.damage,
                    lifetime = 5,
                    ownerEntity = world.Value.PackEntity(ent)
                });
        }

    }
    public struct NetActionMessage : NetworkMessage
    {
        public ushort entityId;
        public byte actionId;
        public Vector2 target;
    }
    public struct ACProjectileLaunch
    {
        public bool selfDestruct;
        public float velocity;
        public float damage;
    }
    public struct AEVActionUse : IEventReplicant
    {
        public bool netReceived;
        public EcsPackedEntity action;
        public Vector2 target;
    }
}