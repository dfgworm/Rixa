using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Health;
using MyEcs.Physics;

[TestFixture]
public class HealthTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsStatic.updateSystems
            .Add(new HealthSystem())
            .Inject(EcsStatic.bus)
            .Init();
        EcsStatic.fixedUpdateSystems
            .Add(new HealthPhysicsSystem())
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
        regen.rate = 5;

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
    [Test]
    public void TouchDamageTest()
    {

        int ent = EcsStatic.world.NewEntity();
        ref var touchDamage = ref EcsStatic.GetPool<ECTouchDamage>().Add(ent);
        touchDamage.dps = 50;
        int ent2 = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(ent2); //just need a component so entity is not empty

        ref var col = ref EcsStatic.bus.NewEvent<EVCollision>();
        col.ent1 = EcsStatic.world.PackEntity(ent);
        col.ent2 = EcsStatic.world.PackEntity(ent2);

        EcsStatic.fixedUpdateSystems.Run();

        int count = 0;
        foreach (int i in EcsStatic.bus.GetEventBodies<EVDamage>(out var pool))
        {
            count++;
            ref var dmg = ref pool.Get(i);
            Assert.AreEqual(touchDamage.dps * Time.fixedDeltaTime, dmg.amount);
            Assert.IsTrue(dmg.dealer.Unpack(EcsStatic.world, out int dealer));
            Assert.AreEqual(ent, dealer);
            Assert.IsTrue(dmg.victim.Unpack(EcsStatic.world, out int victim));
            Assert.AreEqual(ent2, victim);
        }

        Assert.AreEqual(1, count);

    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

}
