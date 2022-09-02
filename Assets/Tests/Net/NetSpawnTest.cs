using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;
using MyEcs.Net;

[TestFixture]
public class NetSpawnTest
{
    NetSpawnSystem system;
    [SetUp]
    public void Setup()
    {
        BaggageSerializer.Init();
        EcsStatic.Load();
        system = new NetSpawnSystem();
        EcsStatic.updateSystems.Add(new SpawnSystem()).Add(system).Inject(EcsStatic.bus).Init();
    }

    [Test]
    public void NetSpawnEntity()
    {
        var payload = new BaggagePayload()
            .Add(new TestBaggage { val = 10 })
            .Add(new TestUpdatableBaggage { num = 3f });

        var msg = new NetSpawnMessage { payload = payload };

        system.ReadPacket(ref msg, 0);

        EVSpawn sp = new EVSpawn();
        ref var ev = ref sp;
        int found = 0;
        foreach (int i in EcsStatic.bus.GetEventBodies<EVSpawn>(out var pool))
        {
            ev = ref pool.Get(i);
            found++;
        }
        Assert.AreEqual(1, found);

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ev.spawnedEntity.Unpack(EcsStatic.world, out int ent));
        Assert.IsTrue(EcsStatic.GetPool<EMSpawned>().Has(ent));

        int[] _ = null;
        int c = EcsStatic.world.GetAllEntities(ref _);
        Assert.AreEqual(1, c);

        ref var comp = ref EcsStatic.GetPool<TestComp>().Get(ent);
        Assert.AreEqual(comp.val, payload.Get<TestBaggage>().val);
        Assert.AreEqual(comp.num, payload.Get<TestUpdatableBaggage>().num);
    }

    [Test]
    public void ConstructMessageAutosync()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var autoSpawn = ref EcsStatic.GetPool<ECNetAutoSpawn>().Add(ent);
        autoSpawn.payload = new BaggagePayload()
            .Add(new TestUpdatableBaggage { num = 15f });

        ref var comp = ref EcsStatic.GetPool<TestComp>().Add(ent);
        comp.num = 1f;

        var msg = system.ConstructMessage(ent);
        Assert.AreEqual(comp.num, msg.payload.Get<TestUpdatableBaggage>().num);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
}