using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Mirror;

using MyEcs.Net;
using MyEcs.Spawn;

[TestFixture]
public class ServiceTest
{
    [SetUp]
    public void Setup()
    {
        BaggageSerializer.Init();
        EcsStatic.Load();
    }
    [Test]
    public void NetIdTest()
    {
        Assert.IsFalse(NetIdService.TryGetEntity(5, out int _));
        Assert.IsFalse(NetIdService.IsNetIdUsed(5));
        Assert.IsFalse(NetIdService.HasEntity(5));

        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<TestComp>().Add(ent);

        Assert.IsFalse(NetIdService.TryGetNetId(ent, out ushort _));

        ushort id = NetIdService.AllocateNetId();
        NetIdService.RegisterEntity(ent, id);

        Assert.IsTrue(NetIdService.TryGetNetId(ent, out ushort recId));
        Assert.AreEqual(id, recId);
        Assert.IsTrue(NetIdService.TryGetEntity(id, out int recEnt));
        Assert.AreEqual(ent, recEnt);
    }
    [Test]
    public void BaggageSerializerTest()
    {
        BaggagePayload payload = new BaggagePayload()
            .Add(new NetIdBaggage(15) )
            .Add(new TestBaggage { val = 5 })
            .Add(new TestBaggage2 { vector = new Vector3(1, 2, 3) })
            .Add(new TestUpdatableBaggage { num = 10f });
        var rec = SendRecievePayload(payload);


        Assert.IsTrue(rec.Has<NetIdBaggage>());
        Assert.IsTrue(rec.Has<TestBaggage>());
        Assert.IsTrue(rec.Has<TestBaggage2>());
        Assert.IsTrue(rec.Has<TestUpdatableBaggage>());
        Assert.AreEqual(payload.Get<NetIdBaggage>().id, rec.Get<NetIdBaggage>().id);
        Assert.AreEqual(payload.Get<TestBaggage>().val, rec.Get<TestBaggage>().val);
        Assert.AreEqual(payload.Get<TestBaggage2>().vector, rec.Get<TestBaggage2>().vector);
        Assert.AreEqual(payload.Get<TestUpdatableBaggage>().num, rec.Get<TestUpdatableBaggage>().num);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
    BaggagePayload SendRecievePayload(BaggagePayload msg)
    {
        var writer = NetworkWriterPool.Get();
        writer.Write(msg);
        byte[] bytes = writer.ToArray();
        var reader = NetworkReaderPool.Get(bytes);
        return reader.Read<BaggagePayload>();
    }

}
