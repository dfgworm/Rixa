using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;

[TestFixture]
public class SpawnTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsStatic.updateSystems.Add(new SpawnSystem()).Inject(EcsStatic.bus).Init();
    }

    [Test]
    public void SpawnEntity()
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload = new BaggagePayload();

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ev.spawnedEntity.Unpack(EcsStatic.world, out int ent));
        Assert.IsTrue(EcsStatic.GetPool<EMSpawned>().Has(ent));
        Assert.AreSame(ev.Payload, EcsStatic.GetPool<EMSpawned>().Get(ent).payload);

        int[] _ = null;
        int c = EcsStatic.world.GetAllEntities(ref _);
        Assert.AreEqual(1, c);

        EcsStatic.world.DelEntity(ent);
    }

    [Test]
    public void UnloadBaggage()
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload = new BaggagePayload().Add(new TestBaggage { val = 5 });

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ev.spawnedEntity.Unpack(EcsStatic.world, out int ent));
        Assert.IsTrue(EcsStatic.GetPool<TestComp>().Has(ent));
        Assert.AreEqual(5, EcsStatic.GetPool<TestComp>().Get(ent).val);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

    public class TestBaggage : IBaggageAutoUnload
    {
        public int val;
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<TestComp>().Add(ent).val = val;
        }
    }
    public struct TestComp
    {
        public int val;
    }
}