using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


#pragma warning disable 0414
public class DebugSystem : IEcsInitSystem, IEcsRunSystem
{
    readonly EcsWorldInject world = default;

    readonly EcsCustomInject<EventBus> bus = default;

    readonly EcsFilterInject<Inc<ECBase>> _filter11 = default;


    public void Init(IEcsSystems systems)
    {

    }

    public void Run(IEcsSystems systems)
    {
        //foreach (int i in _filter11.Value)
        //    Update(ref _filter11.Pools.Inc1.Get(i));
        foreach (int ev in bus.Value.GetEventBodies<EVDebugPrint>(out var pool))
            Print(ref pool.Get(ev));
    }

    void Print(ref EVDebugPrint comp)
    {
        Debug.Log(comp.str);
    }

}
public struct ECDebugName
{
    public string name;
}
public struct EVDebugPrint : IEventReplicant
{
    public string str;
}