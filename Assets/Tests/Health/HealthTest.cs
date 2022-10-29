using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Health;

[TestFixture]
public class HealthTest
{
    [SetUp]
    public void Setup()
    {
        NetStatic.SetIsServer();
        EcsStatic.Load();
        EcsStatic.updateSystems
            .Add(new HealthSystem())
            .Inject(EcsStatic.bus)
            .Init();
    }

    [Test]
    public void RegenTest()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var hp = ref EcsStatic.GetPool<ECHealth>().Add(ent);
        hp.max = 200;
        hp.Current = 100;
        ref var regen = ref EcsStatic.GetPool<ECHealthRegen>().Add(ent);
        regen.regen = 5;

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(hp.Current > 100);
    }
    [Test]
    public void DamageTest()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var hp = ref EcsStatic.GetPool<ECHealth>().Add(ent);
        hp.max = 200;
        hp.Current = 100;

        ref var dmg = ref EcsStatic.bus.NewEvent<EVDamage>();
        dmg.amount = 50;
        dmg.victim = EcsStatic.world.PackEntity(ent);

        EcsStatic.updateSystems.Run();

        Assert.AreEqual(50, hp.Current);
    }

    [TearDown]
    public void TearDown()
    {
        NetStatic.Reset();
        EcsStatic.Unload();
    }

}
