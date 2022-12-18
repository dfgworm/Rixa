using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Actions;
using MyEcs.Net;

[TestFixture]
public class ActionTest
{
    [SetUp]
    public void Setup()
    {
        NetStatic.SetIsServer();
        EcsStatic.Load();
        EcsActionService.Load();
    }

    [Test]
    public void ActionNetMessage()
    {
        var system = new NetActionSystem();
        EcsStatic.updateSystems
            .Add(system)
            .Inject(EcsStatic.bus)
            .Init();

        int ent = EcsStatic.world.NewEntity();
        NetIdService.RegisterEntity(ent, NetIdService.AllocateNetId());
        int ac = EcsActionService.CreateAction(ent);

        ref var useEv = ref EcsStatic.bus.NewEvent<AEVUse>();
        useEv.action = EcsActionService.acWorld.PackEntity(ac);
        useEv.target = new ActionTargetContainer {
            type = ActionTargetType.entity,
            entity = EcsStatic.world.PackEntity(ent),
        };

        Assert.IsTrue(system.TryGetMessage(useEv, out NetActionMessage msg));
        system.ReadMsg(msg, 0);
        int count = 0;
        foreach(int ev in EcsStatic.bus.GetEventBodies<AEVUse>(out var pool))
        {
            ref var use = ref pool.Get(ev);
            Assert.IsTrue(use.action.Unpack(EcsActionService.acWorld, out int a));
            Assert.AreEqual(ac, a);
            Assert.IsTrue(use.target.entity.Unpack(EcsStatic.world, out int e));
            Assert.AreEqual(ent, e);
            count++;
        }
        Assert.AreEqual(2, count);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
        EcsActionService.Unload();
        NetStatic.Reset();
    }

}
