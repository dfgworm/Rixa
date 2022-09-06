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
public class InterpolationTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
    }

    [Test]
    public void PositionInterpolation()
    {
        var pos = new Vector2(10, 5);

        //initial condition
        var inter = new ECInterpolatePositionReceive { };
        inter.AutoReset(ref inter);
        inter.Reset(pos, 0.5f);

        Assert.AreEqual(pos, inter.GetPosition(0.5f));
        Assert.AreEqual(pos, inter.GetPosition(2f));
        Assert.AreEqual(pos, inter.GetPosition(0));

        //adding a snapshot
        var pos2 = new Vector2(5, -5);

        inter.AddSnapshot(pos2, 1f);

        Assert.AreEqual(pos, inter.GetPosition(0));
        Assert.AreEqual(pos2, inter.GetPosition(2f));
        Assert.AreEqual(Vector2.Lerp(pos, pos2, 0.5f), inter.GetPosition(0.75f));

        //adding another snapshot
        var pos3 = new Vector2(15, -150);

        inter.AddSnapshot(pos3, 2f);

        Assert.AreEqual(pos, inter.GetPosition(0));
        Assert.AreEqual(pos3, inter.GetPosition(3f));
        Assert.AreEqual(Vector2.Lerp(pos2, pos3, 0.5f), inter.GetPosition(1.5f));

        //adding more snapshots than buffer can hold
        var pos4 = new Vector2(0, 0);
        for (int i = 0; i < InterpolatedPositionSystem.snapshotMaxCount - 1; i++)
        {
            inter.AddSnapshot(pos4, 2 + i);
        }

        Assert.AreEqual(pos3, inter.GetPosition(0));
        Assert.AreEqual(pos4, inter.GetPosition(10));

        //adding obsolete snapshot
        inter.AddSnapshot(pos, 0);

        Assert.AreEqual(pos3, inter.GetPosition(0));
        Assert.AreEqual(pos4, inter.GetPosition(10));

        //adding another snapshot, filling the last slot with pos4
        inter.AddSnapshot(pos4, 2 + InterpolatedPositionSystem.snapshotMaxCount);

        Assert.AreEqual(pos4, inter.GetPosition(0));

    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

}
