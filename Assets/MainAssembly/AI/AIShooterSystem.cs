using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Act;

public class AIShooterSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECShooterAI, ECSearchTarget, ECPosition>> targetSearchFilter = default;
    readonly EcsFilterInject<Inc<ECShooterAI, ECFollowAI, ECPosition>> aiFilter = default;
    readonly EcsFilterInject<Inc<ECShooterAI, ECActs, ECPosition>> aiActFilter = default;


    public void Run(IEcsSystems systems)
    {
        foreach (int i in targetSearchFilter.Value)
            SearchTarget(EcsStatic.GetEntity(i), ref targetSearchFilter.Pools.Inc1.Get(i), ref targetSearchFilter.Pools.Inc2.Get(i));
        foreach (int i in aiFilter.Value)
            ProcessFollower(EcsStatic.GetEntity(i), ref aiFilter.Pools.Inc1.Get(i), ref aiFilter.Pools.Inc2.Get(i));
        foreach (int i in aiActFilter.Value)
            ProcessAttack(EcsStatic.GetEntity(i), ref aiFilter.Pools.Inc1.Get(i));
            
    }

    void SearchTarget(EcsEntity ent, ref ECShooterAI shooter, ref ECSearchTarget search)
    {
        if (shooter.target.Unpack(EcsStatic.world, out int _))
            return;
        shooter.target = search.foundTarget;
    }
    void ProcessFollower(EcsEntity ent, ref ECShooterAI shooter, ref ECFollowAI follow)
    {
        follow.target = shooter.target;
    }
    void ProcessAttack(EcsEntity ent, ref ECShooterAI shooter)
    {
        foreach(int i in ent.Get<ECActs>().acts)
        {
            EcsEntity act = ActService.GetAct(i);
            if (!act.Has<ACAttackAI>())
                continue;
            act.Get<ACAttackAI>().target = shooter.target;
        }
    }
    
}

public static class AIShooterPipe {
    public static void BuildEntity(EcsEntity ent, FloatRange distance)
    {
        ent.SafeAdd<ECShooterAI>();
        ent.Add<ECFollowAI>().distance = distance;
    }
    public static void BuildAct(EcsEntity act, FloatRange distance)
    {
        act.GetActOwner().SafeAdd<ECShooterAI>();
        ref var attack = ref act.Add<ACAttackAI>();
        attack.distance = distance;
        attack.usageTargetType = ActTargetType.point;
    }
}


public struct ECShooterAI
{
    public EcsPackedEntity target;
}