using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


public class BaseSystem : IEcsInitSystem, IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECBase, ECBase>, Exc<ECBase>> _filter11 = default;


    public void Init(IEcsSystems systems)
    {
        
    }

    public void Run(IEcsSystems systems)
    {
        foreach (int i in _filter11.Value)
            Update(ref _filter11.Pools.Inc1.Get(i));
    }

    void Update(ref ECBase comp)
    {

    }

}
public struct ECBase
{

}
