using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

[TestFixture]
public class CollisionTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
    }

    //[Test]
    //public void PositionInterpolation()
    //{

    //}

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

}
