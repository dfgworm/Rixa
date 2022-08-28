using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;

[TestFixture]
public class DestroyTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsStatic.updateSystems.Add(new DestroySystem()).Inject(EcsStatic.bus).Init();
    }

    [Test]
    public void DestroyEntity()
    {
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.world.GetPool<ECDestroy>().Add(ent);

        EcsStatic.updateSystems.Run();

        int[] _ = null;
        int c = EcsStatic.world.GetAllEntities(ref _);
        Assert.AreEqual(0, c);
    }
    [Test]
    public void DelayedDestroyEntity()
    {
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.world.GetPool<ECDestroyDelayed>().Add(ent);

        EcsStatic.updateSystems.Run();

        Assert.IsFalse(EcsStatic.world.GetPool<ECDestroyDelayed>().Has(ent));
        Assert.IsTrue(EcsStatic.world.GetPool<ECDestroy>().Has(ent));

        EcsStatic.updateSystems.Run();

        int[] _ = null;
        int c = EcsStatic.world.GetAllEntities(ref _);
        Assert.AreEqual(0, c);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
}