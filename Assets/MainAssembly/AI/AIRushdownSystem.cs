using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class AIRushdownSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECFollowAI, ECPosition>> targetSearchFilter = default;


    public void Run(IEcsSystems systems)
    {
        //foreach (int i in targetSearchFilter.Value)
        //    SearchTarget(i, ref targetSearchFilter.Pools.Inc1.Get(i), ref targetSearchFilter.Pools.Inc2.Get(i));
    }

    void SearchTarget(int ent, ref ECSearchTarget search)
    {
        
    }
}

public struct ECRushdownAI
{
    public EcsPackedEntity target;
}