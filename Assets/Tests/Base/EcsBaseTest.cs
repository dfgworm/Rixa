using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;

[TestFixture]
public class EcsBaseTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
    }

    [Test]
    public void SetEntity()
    {
        var go = new GameObject();

        Assert.IsFalse(EcsGameObjectService.TryGetEntity(go, out int _));

        var ent = EcsStatic.world.NewEntity();
        EcsGameObjectService.SetEntity(go, ent);

        var res = EcsGameObjectService.GetEntity(go);
        Assert.AreEqual(res, ent);
        EcsStatic.world.DelEntity(ent);
    }
    [Test]
    public void SetGameObject()
    {
        var ent = EcsStatic.world.NewEntity();

        Assert.IsNull(EcsGameObjectService.GetGameObject(ent));

        var go = new GameObject();
        EcsGameObjectService.SetGameObject(ent, go);

        var res = EcsGameObjectService.GetGameObject(ent);
        Assert.AreEqual(res, go);
        EcsStatic.world.DelEntity(ent);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
}