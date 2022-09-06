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
    [Test]
    public void FilterEnumWithSelf()
    {
        int totalCycles = 10;
        int expectedPasses = 0;
        var filter = EcsStatic.world.Filter<TestComp1>().Inc<TestComp2>().End();
        int[] ents = new int[totalCycles];
        for (int cycle = 0; cycle < totalCycles; cycle++)
        {
            expectedPasses += cycle;

            int entDummy1 = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<TestComp1>().Add(entDummy1);

            int ent = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<TestComp1>().Add(ent);
            EcsStatic.GetPool<TestComp2>().Add(ent);
            ents[cycle] = ent;

            entDummy1 = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<TestComp2>().Add(entDummy1);
            entDummy1 = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<TestComp1>().Add(entDummy1);


            int passes = 0;
            foreach (var pair in filter.EnumPairsWithSelf())
            {
                passes++;
                Assert.AreNotEqual(pair.i, pair.j);
                Assert.Contains(pair.i, ents);
                Assert.Contains(pair.j, ents);
            }
            Assert.AreEqual(expectedPasses, passes);
        }
    }

    public struct TestComp1
    {
    }
    public struct TestComp2
    {
    }
    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
}