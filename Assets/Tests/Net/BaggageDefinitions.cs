using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Mirror;

using MyEcs.Spawn;
public class TestBaggage : IBaggageAutoUnload
{
    public int val;
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<TestComp>().SoftAdd(ent).val = val;
    }
}
public class TestBaggage2 : IBaggageAutoUnload
{
    public Vector3 vector;
    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<TestComp>().SoftAdd(ent).vector = vector;
    }
}
public class TestUpdatableBaggage : IUpdatableBaggage, IBaggageAutoUnload
{
    public float num;
    public bool IsUpToDate(EcsWorld world, int ent) => num == world.GetPool<TestComp>().Get(ent).num;

    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<TestComp>().SoftAdd(ent).num = num;
    }

    public void LoadToBaggage(EcsWorld world, int ent) => num = world.GetPool<TestComp>().Get(ent).num;
}
public struct TestComp
{
    public int val;
    public Vector3 vector;
    public float num;
}