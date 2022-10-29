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
public class NetDestroyTest
{
    NetDestroySystem system;
    [SetUp]
    public void Setup()
    {
        BaggageSerializer.Init();
        EcsStatic.Load();
        system = new NetDestroySystem();
        EcsStatic.updateSystems.Add(system).Add(new DestroySystem()).Inject(EcsStatic.bus).Init();
        NetStatic.SetIsClient();
    }

    [Test]
    public void Destroy()
    {
        ushort netId = 1;
        int ent = EcsStatic.world.NewEntity();
        NetIdService.RegisterEntity(ent, netId);
        var packed = EcsStatic.world.PackEntity(ent);
        var msg = new NetDestroyMessage();
        msg.netId = netId;
        system.ReadDestroyPacket(msg);

        EcsStatic.updateSystems.Run();

        Assert.IsFalse(packed.Unpack(EcsStatic.world, out int _));
    }
    [Test]
    public void DelayedDestroy()
    {
        ushort netId = 1;
        var msg = new NetDestroyMessage();
        msg.netId = netId;
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<DummyComp>().Add(ent);
        var packed = EcsStatic.world.PackEntity(ent);
        system.ReadDestroyPacket(msg);

        EcsStatic.updateSystems.Run();
        EcsStatic.updateSystems.Run();

        Assert.IsTrue(packed.Unpack(EcsStatic.world, out int _));
        NetIdService.RegisterEntity(ent, netId);

        EcsStatic.updateSystems.Run();

        Assert.IsFalse(packed.Unpack(EcsStatic.world, out int _));
    }

    public struct DummyComp
    {}

    [TearDown]
    public void TearDown()
    {
        NetStatic.Reset();
        EcsStatic.Unload();
    }
}