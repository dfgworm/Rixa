using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


public class RotationSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECRotation, ECDestinationRotation>> destinationFilter = default;

    public void Run(IEcsSystems systems)
    {
        foreach (int i in destinationFilter.Value)
            RotateTo(ref destinationFilter.Pools.Inc1.Get(i), ref destinationFilter.Pools.Inc2.Get(i));
    }

    void RotateTo(ref ECRotation rot, ref ECDestinationRotation dest)
    {
        rot.rotation = Mathf.MoveTowardsAngle(rot.rotation, dest.destination, dest.speed*Time.deltaTime);
    }

}
public struct ECRotation
{
    public float rotation;
}
public struct ECDestinationRotation
{
    public float destination;
    public float speed;

}

public class RotationBaggage : MyEcs.Spawn.IUpdatableBaggage
{
    public float rotation;
    public void UpdateThis(EcsWorld world, int ent)
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