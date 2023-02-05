using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

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
                go.transform.position = pos.position2.Vec3();
        }
    }
}
public class PositionBaggage : IUpdatableBaggage
{
    public Vector2 position;
    public void LoadToBaggage(EcsWorld world, int ent)
    {
        var pool = world.GetPool<ECPosition>();
        position = pool.Get(ent).position2;
    }
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<ECPosition>().Get(ent).position2 = position;
    }
    public bool IsUpToDate(EcsWorld world, int ent)
    {
        return world.GetPool<ECPosition>().Get(ent).position2.FuzzyEquals(position);
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
