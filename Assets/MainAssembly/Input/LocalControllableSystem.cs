using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Actions;

public class LocalControllableSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECTargetVelocity, ECLocalControllable>> moveFilter = default;
    readonly EcsFilterInject<Inc<ACInputType>> abilityFilter = "actions";

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsWorldInject world = default;
    readonly EcsWorldInject acWorld = "actions";

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
        foreach (int ev in bus.Value.GetEventBodies<InpActionUse>(out var pool))
            ProcessAction(ref pool.Get(ev));
    }

    void ControlMove(ref ECTargetVelocity mover)
    {
        if (bus.Value.HasEventSingleton<InpMovement>())
            mover.direction = bus.Value.GetEventBodySingleton<InpMovement>().dpos.normalized;
        else
            mover.direction = Vector2.zero;
    }
    void ProcessAction(ref InpActionUse inputEvent)
    {
        if (!inputEvent.action.Unpack(EcsActionService.acWorld, out int ac))
            return;
        ref ACInputType localContr = ref EcsActionService.GetPool<ACInputType>().Get(ac);

        ref var ev = ref bus.Value.NewEvent<AEVUse>();
        ev.action = acWorld.Value.PackEntity(ac);
        ev.target = new ActionTargetContainer { type = localContr.targetType };

        if (ev.target.type == ActionTargetType.point) {
            ev.target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos;
        } else if (ev.target.type == ActionTargetType.direction)
        {
            if (EcsActionService.TryGetEntity(ac, out int ent))
            {
                var pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
                ev.target.vector = bus.Value.GetEventBodySingleton<InpMouseWorldPosition>().pos - pos;
            }
        } else if (ev.target.type == ActionTargetType.entity)
            if (bus.Value.HasEventSingleton<InpEntityMouseHover>())
                ev.target.entity = bus.Value.GetEventBodySingleton<InpEntityMouseHover>().entity;
    }
}

public struct ECLocalControllable
{
}
public struct ACInputType //this should have more customization options, including it's own enum
{
    public ActionTargetType targetType;
}