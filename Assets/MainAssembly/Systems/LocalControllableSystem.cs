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

    readonly EcsFilterInject<Inc<ECAcceleration, ECLocalControllable>> moveFilter = default;
    readonly EcsFilterInject<Inc<ACLocalControllable>> abilityFilter = "actions";

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsWorldInject acWorld = "actions";

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
        if (bus.Value.HasEventSingleton<InpActionUse>())
            foreach (int i in abilityFilter.Value)
                UseActions(i, ref abilityFilter.Pools.Inc1.Get(i));
    }

    void ControlMove(ref ECAcceleration mover)
    {
        if (bus.Value.HasEventSingleton<InpMovementInput>())
            mover.direction = bus.Value.GetEventBodySingleton<InpMovementInput>().dpos.normalized;
        else
            mover.direction = Vector2.zero;
    }
    void UseActions(int ent, ref ACLocalControllable localContr)
    {
        ref var inpEv = ref bus.Value.GetEventBodySingleton<InpActionUse>();
        ref var ev = ref bus.Value.NewEvent<AEVActionUse>();
        ev.action = acWorld.Value.PackEntity(ent);
        ev.target = inpEv.target;
    }
}

public struct ECLocalControllable
{
}
public struct ACLocalControllable
{
}