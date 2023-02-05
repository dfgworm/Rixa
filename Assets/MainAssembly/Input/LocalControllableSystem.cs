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
    readonly EcsFilterInject<Inc<ACLocalControllable>> abilityFilter = "actions";

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsWorldInject world = default;
    readonly EcsWorldInject acWorld = "actions";

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
        if (bus.Value.HasEventSingleton<InpActionUse>())
            foreach (int i in abilityFilter.Value)
                UseAction(i, ref abilityFilter.Pools.Inc1.Get(i));
    }

    void ControlMove(ref ECTargetVelocity mover)
    {
        if (bus.Value.HasEventSingleton<InpMovement>())
            mover.direction = bus.Value.GetEventBodySingleton<InpMovement>().dpos.normalized;
        else
            mover.direction = Vector2.zero;
    }
    void UseAction(int ac, ref ACLocalControllable localContr)
    {
        ref var inpEv = ref bus.Value.GetEventBodySingleton<InpActionUse>();

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
public struct ACLocalControllable
{
    public ActionTargetType targetType;
}