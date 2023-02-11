using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Acts;

public class LocalControllableSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECTargetVelocity, ECLocalControllable>> moveFilter = default;
    readonly EcsFilterInject<Inc<ACInputType>> abilityFilter = "act";

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsWorldInject world = default;
    readonly EcsWorldInject actWorld = "act";

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
        foreach (int ev in bus.Value.GetEventBodies<InpActUse>(out var pool))
            ProcessAct(ref pool.Get(ev));
    }

    void ControlMove(ref ECTargetVelocity mover)
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

        ref var ev = ref bus.Value.NewEvent<AEVUse>();
        ev.act = actWorld.Value.PackEntity(ac);
        ev.target = new ActTargetContainer { type = localContr.targetType };

        if (ev.target.type == ActTargetType.point) {
            ev.target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos;
        } else if (ev.target.type == ActTargetType.direction)
        {
            if (ActService.TryGetEntity(ac, out int ent))
            {
                var pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
                ev.target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos - pos;
            }
        } else if (ev.target.type == ActTargetType.entity)
            if (bus.Value.HasEventSingleton<InpEntityMouseHover>())
                ev.target.entity = bus.Value.GetEventBodySingleton<InpEntityMouseHover>().entity;
    }
}

public struct ECLocalControllable
{
}
public struct ACInputType //this should have more customization options, including it's own enum
{
    public ActTargetType targetType;
}