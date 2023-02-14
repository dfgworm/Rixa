using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class AITargetSearchSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECSearchTarget, ECPosition>> targetSearchFilter = default;
    readonly EcsFilterInject<Inc<ECPotentialTarget, ECPosition>> potentialTargetFilter = default;


    public void Run(IEcsSystems systems)
    {
        foreach (int i in targetSearchFilter.Value)
            SearchTarget(EcsStatic.GetEntity(i), ref targetSearchFilter.Pools.Inc1.Get(i));
    }

    void SearchTarget(EcsEntity ent, ref ECSearchTarget search)
    {
        if (search.foundTarget.Unpack(EcsStatic.world, out int _))
            return;
        var pos = ent.Get<ECPosition>().position2;
        foreach(int targetEnt in potentialTargetFilter.Value)
        {
            var targetPos = EcsStatic.GetPool<ECPosition>().Get(targetEnt).position2;
            if ((targetPos - pos).magnitude > search.distance)
                continue;
            search.foundTarget = EcsStatic.world.PackEntity(targetEnt);
            break;
        }
    }
}

public struct ECSearchTarget
{
    public float distance;
    public EcsPackedEntity foundTarget;
}
public struct ECPotentialTarget
{
}