#define DEBUG
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;

[TestFixture]
public class BaggageTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
    }

    [Test]
    public void BaggageListTest()
    {
        BaggagePayload payload = new BaggagePayload();
        Assert.AreEqual(0, payload.List.Count);
        Assert.IsFalse(payload.Has<TestBaggage>());
        var test = new TestBaggage();
        payload.Add(test);

        Assert.AreEqual(1, payload.List.Count);
        Assert.IsTrue(payload.Has<TestBaggage>());
        Assert.IsFalse(payload.Has<TestBaggage2>());

        var test2 = new TestBaggage2();
        payload.Add(test2);

        Assert.AreEqual(2, payload.List.Count);
        Assert.IsTrue(payload.Has<TestBaggage2>());

        Assert.Throws<System.Exception>(() => payload.Add(new TestBaggage()), typeof(TestBaggage).Name + " type dublicate adding to payload");

        Assert.AreSame(test, payload.Get<TestBaggage>());
        Assert.AreSame(test2, payload.Get<TestBaggage2>());
    }
    [Test]
    public void UnloadTest()
    {
        int ent = EcsStatic.world.NewEntity();

        BaggagePayload payload = new BaggagePayload();
        payload.Add(new TestBaggage());
        payload.UnloadToWorld(EcsStatic.world, ent);

        Assert.IsTrue(EcsStatic.GetPool<TestComp>().Has(ent));
    }
    [Test]
    public void UpdatableBaggageTest()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var comp = ref EcsStatic.GetPool<TestComp>().Add(ent);

        BaggagePayload payload = new BaggagePayload();
        var baggage = new TestUpdatableBaggage();
        payload.Add(baggage);

        baggage.val = 5;
        payload.UnloadToWorld(EcsStatic.world, ent);
        Assert.AreEqual(baggage.val, comp.val);

        List<IBaggage> bufferList = new List<IBaggage>();
        Assert.AreEqual(0, payload.GetNeedsUpdate(EcsStatic.world, ent, bufferList));
        comp.val = 15;
        Assert.AreEqual(1, payload.GetNeedsUpdate(EcsStatic.world, ent, bufferList));
        payload.UpdateThis(EcsStatic.world, ent);
        Assert.AreEqual(comp.val, baggage.val);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

    public class TestBaggage : IBaggage
    {
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<TestComp>().Add(ent);
        }
    }
    public class TestBaggage2 : IBaggage
    {
        public void UnloadToWorld(EcsWorld world, int ent) {}
    }

    public class TestUpdatableBaggage : IUpdatableBaggage
    {
        public int val;
        public bool IsUpToDate(EcsWorld world, int ent) => val == world.GetPool<TestComp>().Get(ent).val;

        public void UnloadToWorld(EcsWorld world, int ent)
        {
            if (world.GetPool<TestComp>().Has(ent))
                world.GetPool<TestComp>().Get(ent).val = val;
            else
                world.GetPool<TestComp>().Add(ent).val = val;
        }

        public void LoadToBaggage(EcsWorld world, int ent) => val = world.GetPool<TestComp>().Get(ent).val;
    }
    public struct TestComp
    {
        public int val;
    }

}