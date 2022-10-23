using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

public class LocalControllableSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECAcceleration, ECLocalControllable>> moveFilter = default;

    readonly EcsCustomInject<EventBus> bus = default;

    public void Run(IEcsSystems systems)
    {
        if (NetStatic.IsServer)
            return;
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
    }

    void ControlMove(ref ECAcceleration mover)
    {
        if (bus.Value.HasEventSingleton<EVMovementInput>())
            mover.direction = bus.Value.GetEventBodySingleton<EVMovementInput>().dpos.normalized;
        else
            mover.direction = Vector2.zero;
    }

}

public struct ECLocalControllable
{
}