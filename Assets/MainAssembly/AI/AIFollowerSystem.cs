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

    readonly EcsFilterInject<Inc<ECFollowTargetAI, ECTarget, ECDesiredVelocity, ECPosition>> moveFilter = default;
    readonly EcsFilterInject<Inc<ACAttackTargetAI>> attackFilter = "act";

    readonly EcsCustomInject<EventBus> bus = default;

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlFollow(i, ref moveFilter.Pools.Inc1.Get(i), ref moveFilter.Pools.Inc2.Get(i), ref moveFilter.Pools.Inc3.Get(i));
        foreach (int i in attackFilter.Value)
            ProcessAct(ActService.Entity(i), ref attackFilter.Pools.Inc1.Get(i));
    }

    void ControlFollow(int ent, ref ECFollowTargetAI follower, ref ECTarget target, ref ECDesiredVelocity mover)
    {
        if (!target.entity.Unpack(EcsStatic.world, out int targetEnt) || !EcsStatic.GetPool<ECPosition>().Has(targetEnt)) {
            mover.direction = Vector2.zero;
            return;
        }
        var pos = EcsStatic.GetPool<ECPosition>().Get(ent).position2;
        var targetPos = EcsStatic.GetPool<ECPosition>().Get(targetEnt).position2;
        var diff = targetPos - pos;
        var dist = diff.magnitude;
        if (dist > follower.maxDistance)
            mover.direction = diff.normalized;
        else if (dist < follower.minDistance)
            mover.direction = -diff.normalized;
        else
            mover.direction = Vector2.zero;

    }
    void ProcessAct(EcsEntity act, ref ACAttackTargetAI attack)
    {
        EcsEntity ent = act.GetEntity();
        Debug.Log($"process attack ai 1 {ent.entity}");
        if (!ent.Has<ECTarget>() || !ent.Get<ECTarget>().entity.UnpackEntity(out EcsEntity targetEnt))
            return;

        Debug.Log("process attack ai 2");
        if (act.Has<ACAmmo>() && act.Get<ACAmmo>().amount.Current < 1)
            return;

        var usage = new ActUsageContainer { targetType = attack.targetType };
        if (usage.targetType == ActTargetType.point)
        {
            if (!targetEnt.Has<ECPosition>())
                return;
            usage.vector = targetEnt.Get<ECPosition>().position2;
        } else if (usage.targetType == ActTargetType.direction)
        {
            if (!targetEnt.Has<ECPosition>())
                return;
            var pos = ent.Get<ECPosition>().position2;
            usage.vector = targetEnt.Get<ECPosition>().position2 - pos;
        } else if (usage.targetType == ActTargetType.entity)
            if (bus.Value.HasEventSingleton<InpEntityMouseHover>())
                usage.entity = targetEnt.Pack();

        ref var used = ref act.SafeAdd<AMUsed>();
        used.usages.Add(usage);
    }
}

public struct ECFollowTargetAI
{
    public float minDistance;
    public float maxDistance;
}
public struct ACAttackTargetAI
{
    public float minDistance;
    public float maxDistance;
    public ActTargetType targetType;
}