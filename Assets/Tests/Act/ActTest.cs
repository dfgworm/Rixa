using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Act;

[TestFixture]
public class ActTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        ActService.Load();
    }

    [Test]
    public void ActionCreation()
    {
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(ent); //to avoid 'empty entity' errors
        int act = ActService.CreateAct(ent);

        int _ent = ActService.GetEntity(act);
        Assert.AreEqual(ent, _ent);

        byte id = ActService.GetActId(act);
        Assert.IsTrue(ActService.TryGetAct(ent, id, out int _act));
        Assert.AreEqual(act, _act);
        Assert.AreEqual(1, ActService.world.GetEntitiesCount());

        ActService.RemoveAct(act);
        Assert.AreEqual(0, ActService.world.GetEntitiesCount());

        act = ActService.CreateAct(ent);
        EcsStatic.world.DelEntity(ent);
        Assert.AreEqual(0, ActService.world.GetEntitiesCount());
    }
    [Test]
    public void ActAmmo()
    {
        EcsStatic.updateSystems.Add(new AmmoLimitationSystem()).Inject(EcsStatic.bus).Init();

        int ent = EcsStatic.world.NewEntity();
        int act = ActService.CreateAct(ent);
        ref var ammo = ref ActService.GetPool<ACAmmo>().Add(act);
        ammo.max = 3;
        ammo.Current = 1;

        ref var usage = ref ActService.GetPool<AMUsed>().Add(act);
        usage.usages.Add(new ActUsageContainer());

        EcsStatic.updateSystems.Run();

        Assert.AreEqual(1, usage.usages.Count);
        Assert.AreEqual(0, ammo.Current);

        EcsStatic.updateSystems.Run();

        Assert.AreEqual(0, usage.usages.Count);

    }
    [Test]
    public void ActChannel()
    {
        EcsStatic.updateSystems.Add(new ActChannellingSystem()).Inject(EcsStatic.bus).Init();


        int ent = EcsStatic.world.NewEntity();
        int act = ActService.CreateAct(ent);
        ref var channeled = ref ActService.GetPool<ACChannelled>().Add(act);
        int finishAct = ActService.CreateAct(ent);
        channeled.finishAct = ActService.world.PackEntity(finishAct);
        channeled.duration = 2;

        ActService.GetPool<AMUsed>().Add(act).usages.Add(new ActUsageContainer { targetType = ActTargetType.point });

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ActService.GetPool<AMChannelingProcess>().Has(act));
        ref var channelProcess = ref ActService.GetPool<AMChannelingProcess>().Get(act);
        Assert.AreEqual(channeled.duration, channelProcess.duration);

        channelProcess.timer = channeled.duration + 1;
        ActService.GetPool<AMUsed>().Del(act);
        EcsStatic.updateSystems.Run();
        Assert.IsFalse(ActService.GetPool<AMChannelingProcess>().Has(act));

        Assert.IsTrue(ActService.GetPool<AMUsed>().Has(finishAct));
        Assert.AreEqual(ActTargetType.point, ActService.GetPool<AMUsed>().Get(finishAct).usages[0].targetType);


    }
    [Test]
    public void InstantDelivery()
    {
        EcsStatic.updateSystems.Add(new InstantADSystem()).Inject(EcsStatic.bus).Init();

        int ent = EcsStatic.world.NewEntity();
        int act = ActService.CreateAct(ent);
        ActService.GetPool<ACInstantDelivery>().Add(act);
        Vector2 point = new Vector2(2, 2);
        ActService.GetPool<AMUsed>().Add(act).usages.Add(new ActUsageContainer {
            targetType = ActTargetType.point,
            vector = point,
        });

        EcsStatic.updateSystems.Run();

        Assert.IsFalse(ActService.GetPool<AMEntityHit>().Has(act));
        Assert.IsTrue(ActService.GetPool<AMPointHit>().Has(act));
        Assert.AreEqual(point, ActService.GetPool<AMPointHit>().Get(act).points[0]);

        int victim = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(victim);
        ActService.GetPool<AMUsed>().SafeAdd(act).usages.Add(new ActUsageContainer
        {
            targetType = ActTargetType.entity,
            entity = EcsStatic.world.PackEntity(victim)
        });

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ActService.GetPool<AMEntityHit>().Has(act));
        var packedVictim = ActService.GetPool<AMEntityHit>().Get(act).victims[0];
        Assert.IsTrue(packedVictim.Unpack(EcsStatic.world, out int _vic));
        Assert.AreEqual(victim, _vic);
    }
    [Test]
    public void ProjectileDelivery()
    {
        EcsStatic.updateSystems.Add(new ProjectileADSystem()).Inject(EcsStatic.bus).Init();

        var pos = new Vector2(2, 1);
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(ent).position2 = pos;
        int act = ActService.CreateAct(ent);
        ref var projAD = ref ActService.GetPool<ACProjectileDelivery>().Add(act);
        projAD.lifetime = 2;
        projAD.selfDestruct = false;
        projAD.velocity = 10;

        Vector2 dir = new Vector2(1, 0);
        ActService.GetPool<AMUsed>().Add(act).usages.Add(new ActUsageContainer
        {
            targetType = ActTargetType.direction,
            vector = dir,
        });

        EcsStatic.updateSystems.Run();

        int proj = -1;
        int[] ents = new int[10];
        int c = EcsStatic.world.GetAllEntities(ref ents);
        for(int i = 0; i < c; i++)
        {
            if (ents[i] == ent)
                continue;
            proj = ents[i];
            break;
        }
        Assert.IsTrue(proj != -1);
        Assert.IsTrue(EcsStatic.GetPool<ECProjectile>().Has(proj));
        ref var ecProj = ref EcsStatic.GetPool<ECProjectile>().Get(proj);
        Assert.IsTrue(ecProj.sourceAct.Unpack(ActService.world, out int unpacked));
        Assert.AreEqual(act, unpacked);
        Assert.AreEqual(projAD.lifetime, EcsStatic.GetPool<ECDestroyDelayed>().Get(proj).time);
        Assert.AreEqual(projAD.velocity*dir, EcsStatic.GetPool<ECVelocity>().Get(proj).velocity);

        int victim = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(victim);

        ref var col = ref EcsStatic.bus.NewEvent<EVCollision>();
        col.ent1 = EcsStatic.world.PackEntity(proj);
        col.ent2 = EcsStatic.world.PackEntity(victim);

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ActService.GetPool<AMPointHit>().Has(act));
        Assert.AreEqual(pos, ActService.GetPool<AMPointHit>().Get(act).points[0]);

        Assert.IsTrue(ActService.GetPool<AMEntityHit>().Has(act));
        var packedVictim = ActService.GetPool<AMEntityHit>().Get(act).victims[0];
        Assert.IsTrue(packedVictim.Unpack(EcsStatic.world, out int _vic));
        Assert.AreEqual(victim, _vic);

    }
    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
        ActService.Unload();
    }

}
