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
    public class NetActionSystem : IEcsInitSystem, IEcsRunSystem
    {

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;

        public void Init(IEcsSystems systems)
        {
            if (NetStatic.IsServer)
                NetworkServer.RegisterHandler<NetActionMessage>(ReadMsgServer);
            else if (NetStatic.IsClient)
                NetworkClient.RegisterHandler<NetActionMessage>(ReadMsgClient);
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVUse>(out var pool))
                TrySendUseEvent(ref pool.Get(i));

        }
        [Server]
        public void ReadMsgServer(NetworkConnection conn, NetActionMessage msg)
        {
            if (!conn.OwnsNetId(msg.entityId))
                return;
            NetCommunication.BroadcastToReadyExcept(conn.connectionId, msg, Channels.Reliable);
            ReadMsg(msg, (float)conn.remoteTimeStamp);
        }
        [Client]
        public void ReadMsgClient(NetActionMessage msg)
        {
            ReadMsg(msg, (float)NetworkClient.connection.remoteTimeStamp);
        }
        public void ReadMsg(NetActionMessage msg, float time)
        {
            if (!NetIdService.TryGetEntity(msg.entityId, out int ent))
                return;
            if (!EcsActionService.TryGetAction(ent, msg.actionId, out int ac))
                return;
            ref var ev = ref bus.Value.NewEvent<AEVUse>();
            ev.netReceived = true;
            ev.action = acWorld.Value.PackEntity(ac);
            ev.target = msg.target;
        }
        void TrySendUseEvent(ref AEVUse useEvent)
        {
            if (!TryGetMessage(useEvent, out NetActionMessage msg))
                return;
            if (NetStatic.IsServer)
                NetCommunication.BroadcastToReady(msg, Channels.Reliable);
            else
                NetCommunication.SendToServer(msg, Channels.Reliable);
        }

        public bool TryGetMessage(AEVUse useEvent, out NetActionMessage msg)
        {
            msg = new NetActionMessage();
            if (useEvent.netReceived || !useEvent.action.Unpack(acWorld.Value, out int ac))
                return false;
            if (!EcsActionService.TryGetEntity(ac, out int ent) || !world.Value.GetPool<ECNetId>().Has(ent))
                return false;
            if (!NetOwnershipService.IsEntityLocalAuthority(ent))
                return false;
            ref var netId = ref world.Value.GetPool<ECNetId>().Get(ent);

            msg.entityId = netId.id;
            msg.actionId = EcsActionService.GetActionId(ac);
            msg.target = useEvent.target;
            return true;
        }
    }
    public struct NetActionMessage : NetworkMessage
    {
        public ushort entityId;
        public byte actionId;
        public ActionTargetContainer target;
    }
    public struct AEVUse : IEventReplicant
    {
        public bool netReceived;
        public EcsPackedEntity action;
        public ActionTargetContainer target;
    }
    public struct AEVEntityHit : IEventReplicant
    {
        public EcsPackedEntity action;
        public EcsPackedEntity victim;
    }
    public struct AEVPointHit : IEventReplicant
    {
        public EcsPackedEntity action;
        public Vector2 point;
    }
    public enum ActionTargetType :byte
    {
        none,
        point,
        direction,
        entity,
    }
    public struct ActionTargetContainer
    {
        public ActionTargetType type;
        public Vector2 vector;
        public EcsPackedEntity entity;
    }
    public static class ActionTargetSerializer
    {
        public static void WriteActionTarget(this NetworkWriter writer, ActionTargetContainer container)
        {
            writer.WriteByte((byte)container.type);
            if (container.type == ActionTargetType.entity)
            {
                NetIdService.TryGetNetId(container.entity, out ushort netId);
                writer.WriteUShort(netId);
            } else if (container.type == ActionTargetType.point || container.type == ActionTargetType.direction) {
                writer.WriteVector2(container.vector);
            }
        }
        public static ActionTargetContainer ReadActionTarget(this NetworkReader reader)
        {
            var container = new ActionTargetContainer();
            container.type = (ActionTargetType)reader.ReadByte();
            if (container.type == ActionTargetType.entity)
            {
                ushort netId = reader.ReadUShort();
                if (NetIdService.TryGetEntity(netId, out int ent))
                    container.entity = EcsStatic.world.PackEntity(ent);
            } else if (container.type == ActionTargetType.point || container.type == ActionTargetType.direction) {
                container.vector = reader.ReadVector2();
            }
            return container;
        }
    }
}