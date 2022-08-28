using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class PositionSystem : IEcsRunSystem
{

    readonly EcsFilterInject<Inc<ECPosition, ECMover>> moveFilter = default;

    public void Run(IEcsSystems systems)
    {
        foreach (int i in moveFilter.Value)
            Move(ref moveFilter.Pools.Inc1.Get(i), ref moveFilter.Pools.Inc2.Get(i));
    }

    void Move(ref ECPosition pos, ref ECMover mover)
    {
        if (!mover.IsMoving)
            return;
        pos.position += mover.direction * mover.speed*Time.deltaTime;
    }

}
public struct ECPosition
{
    public Vector2 position;
}
public struct ECMover
{
    public Vector2 direction;
    public float speed;

    public bool IsMoving => direction != Vector2.zero && Math.Abs(speed) > 1e-05;

}
public class PositionBaggage : MyEcs.Spawn.IUpdatableBaggage
{
    public Vector2 position;
    public void UpdateThis(EcsWorld world, int ent)
    {
        var pool = world.GetPool<ECPosition>();
        position = pool.Get(ent).position;
    }
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<ECPosition>().SoftAdd(ent).position = position;
    }
    public bool IsUpToDate(EcsWorld world, int ent)
    {
        return world.GetPool<ECPosition>().Get(ent).position.FuzzyEquals(position);
    }
}
