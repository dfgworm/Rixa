using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class AITargetSearchSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECTarget, ECSearchTarget, ECPosition>> targetSearchFilter = default;
    readonly EcsFilterInject<Inc<ECPotentialTarget, ECPosition>> potentialTargetFilter = default;


    public void Run(IEcsSystems systems)
    {
        foreach (int i in targetSearchFilter.Value)
            SearchTarget(i, ref targetSearchFilter.Pools.Inc1.Get(i), ref targetSearchFilter.Pools.Inc2.Get(i));
    }

    void SearchTarget(int ent, ref ECTarget ecTarget, ref ECSearchTarget search)
    {
        if (ecTarget.entity.Unpack(EcsStatic.world, out int _))
            return;
        var pos = EcsStatic.GetPool<ECPosition>().Get(ent).position2;
        foreach(int targetEnt in potentialTargetFilter.Value)
        {
            var targetPos = EcsStatic.GetPool<ECPosition>().Get(targetEnt).position2;
            if ((targetPos - pos).magnitude > search.distance)
                continue;
            ecTarget.entity = EcsStatic.world.PackEntity(targetEnt);
            break;
        }
    }
}

public struct ECTarget
{
    public EcsPackedEntity entity;
}
public struct ECSearchTarget
{
    public float distance;
}
public struct ECPotentialTarget
{
}