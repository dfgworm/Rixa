using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Act;

public class LocalControllableSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECDesiredVelocity, ECLocalControllable>> moveFilter = default;

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsWorldInject world = default;

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
        foreach (int ev in bus.Value.GetEventBodies<InpActUse>(out var pool))
            ProcessAct(ref pool.Get(ev));
    }

    void ControlMove(ref ECDesiredVelocity mover)
    {
        if (bus.Value.HasEventSingleton<InpMovement>())
            mover.direction = bus.Value.GetEventBodySingleton<InpMovement>().dpos.normalized;
        else
            mover.direction = Vector2.zero;
    }
    void ProcessAct(ref InpActUse inputEvent)
    {
        if (!inputEvent.act.Unpack(ActService.world, out int ac))
            return;
        ref ACInputType localContr = ref ActService.GetPool<ACInputType>().Get(ac);

        ref var used = ref ActService.GetPool<AMUsed>().SafeAdd(ac);
        var target = new ActUsageContainer { targetType = localContr.targetType };

        if (target.targetType == ActTargetType.point) {
            target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos;
        } else if (target.targetType == ActTargetType.direction)
        {
            int ent = ActService.GetEntity(ac);
            var pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
            target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos - pos;
        } else if (target.targetType == ActTargetType.entity)
            if (bus.Value.HasEventSingleton<InpEntityMouseHover>())
                target.entity = bus.Value.GetEventBodySingleton<InpEntityMouseHover>().entity;
        used.usages.Add(target);
    }
}

public struct ECLocalControllable
{
}
public struct ACInputType //this should have more customization options, including it's own enum
{
    public ActTargetType targetType;
}