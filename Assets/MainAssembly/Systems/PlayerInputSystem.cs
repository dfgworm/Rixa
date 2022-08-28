using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class PlayerInputSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECMover, ECLocalPlayer>> moveFilter = default;



    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            ControlMove(ref moveFilter.Pools.Inc1.Get(i));
    }

    void ControlMove(ref ECMover mover)
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        mover.direction = new Vector2(x, y).normalized;
    }

}
