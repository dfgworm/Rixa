using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Act;

public class AIFollowerSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECFollowAI, ECDesiredVelocity, ECPosition>> moveFilter = default;
    readonly EcsFilterInject<Inc<ACAttackAI>> attackFilter = "act";

    readonly EcsCustomInject<EventBus> bus = default;

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlFollow(EcsStatic.GetEntity(i), ref moveFilter.Pools.Inc1.Get(i), ref moveFilter.Pools.Inc2.Get(i));
        foreach (int i in attackFilter.Value)
            ProcessAct(ActService.GetAct(i), ref attackFilter.Pools.Inc1.Get(i));
    }

    void ControlFollow(EcsEntity ent, ref ECFollowAI follower, ref ECDesiredVelocity mover)
    {
        if (!follower.target.UnpackEntity(out EcsEntity targetEnt) || !targetEnt.Has<ECPosition>()) {
            mover.direction = Vector2.zero;
            return;
        }
        var pos = ent.Get<ECPosition>().position2;
        var targetPos = targetEnt.Get<ECPosition>().position2;
        var diff = targetPos - pos;
        var dist = diff.magnitude;
        if (dist > follower.distance.max)
            mover.direction = diff.normalized;
        else if (dist < follower.distance.min)
            mover.direction = -diff.normalized;
        else
            mover.direction = Vector2.zero;

    }
    void ProcessAct(EcsEntity act, ref ACAttackAI attack)
    {
        if (!attack.target.UnpackEntity(out EcsEntity targetEnt))
            return;

        if (act.Has<ACAmmo>() && act.Get<ACAmmo>().amount.Current < 1)
            return;
        EcsEntity ent = act.GetActOwner();
        var pos = ent.Get<ECPosition>().position2;
        if (!targetEnt.Has<ECPosition>())
            return;
        var targetPos = targetEnt.Get<ECPosition>().position2;
        var dist = (targetPos - pos).magnitude;
        if (!attack.distance.IsInRange(dist))
            return;

        var usage = new ActUsageContainer { targetType = attack.usageTargetType };
        if (usage.targetType == ActTargetType.point)
        {
            usage.vector = targetPos;
        } else if (usage.targetType == ActTargetType.direction)
        {
            usage.vector = targetPos - pos;
        } else if (usage.targetType == ActTargetType.entity)
            if (bus.Value.HasEventSingleton<InpEntityMouseHover>())
                usage.entity = targetEnt.Pack();

        ref var used = ref act.SafeAdd<AMUsed>();
        used.usages.Add(usage);
    }
}

public struct ECFollowAI
{
    public FloatRange distance;
    public EcsPackedEntity target;
}
public struct ACAttackAI
{
    public FloatRange distance;
    public ActTargetType usageTargetType;
    public EcsPackedEntity target;
}