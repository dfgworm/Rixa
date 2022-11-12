using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using Mirror;
using MyEcs.Net;
using MyEcs.Spawn;
using MyEcs.Physics;

public static class PositionPipe
{
    public struct PosArgs
    {
        public bool positionToTransform;
    }
    public static void BuildPosition(EcsWorld world, int ent, bool positionToTransform = false)
    {
        ref var pos = ref world.GetPool<ECPosition>().Add(ent);
        world.GetPool<EMSpawned>().Get(ent).payload.EnsureBaggageAndUnload<PositionBaggage>(world, ent);
        if (positionToTransform)
        {
            world.GetPool<ECPositionToTransform>().Add(ent);
            var go = EcsGameObjectService.GetGameObject(ent);
            if (go != null)
                go.transform.position = pos.position.Vec3();
        }
    }
    public struct NetPosArgs
    {
        public float syncPeriodFromServer;
    }
    public static void BuildNetPosition(EcsWorld world, int ent, NetPosArgs args)
    {
        if (world.GetPool<ECNetOwner>().Get(ent).IsLocalAuthority)
            BuildLocalAuthority(world, ent, args);
        else
            BuildRemoteAuthority(world, ent, args);

    }
    static void BuildLocalAuthority(EcsWorld world, int ent, NetPosArgs args)
    {
        ref var send = ref world.GetPool<ECSyncSend>().SoftAdd(ent);
        send.Payload.Add(new PositionBaggage());
    }
    static void BuildRemoteAuthority(EcsWorld world, int ent, NetPosArgs args)
    {
        if (NetStatic.IsServer)
        {
            ref var netOwner = ref world.GetPool<ECNetOwner>().Get(ent);
            var pb = PlayerBehaviour.GetById(netOwner.playerId);
            world.GetPool<ECSyncReceive>().SoftAdd(ent);
            ref var send = ref world.GetPool<ECInterpolatePositionSend>().Add(ent);
            send.sendPeriod = args.syncPeriodFromServer;
            send.exceptionConnectionId = pb.connection.connectionId;
        }
        else
        {
            world.GetPool<ECInterpolatePositionReceive>().Add(ent)
                .Reset(world.GetPool<ECPosition>().Get(ent).position, (float)NetworkTime.time);
        }
    }
}
public class PositionBaggage : IUpdatableBaggage
{
    public Vector2 position;
    public void LoadToBaggage(EcsWorld world, int ent)
    {
        var pool = world.GetPool<ECPosition>();
        position = pool.Get(ent).position;
    }
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<ECPosition>().Get(ent).position = position;
    }
    public bool IsUpToDate(EcsWorld world, int ent)
    {
        return world.GetPool<ECPosition>().Get(ent).position.FuzzyEquals(position);
    }
}
public class RotationBaggage : IUpdatableBaggage //this one doesn't belong here, but i don't need it yet
{
    public float rotation;
    public void LoadToBaggage(EcsWorld world, int ent)
    {
        var pool = world.GetPool<ECRotation>();
        rotation = pool.Get(ent).rotation;
    }
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<ECRotation>().SoftAdd(ent).rotation = rotation;
    }
    public bool IsUpToDate(EcsWorld world, int ent)
    {
        return world.GetPool<ECRotation>().Get(ent).rotation == rotation;
    }
}
