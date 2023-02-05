using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Actions;

[TestFixture]
public class ActionTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsActionService.Load();
    }


    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
        EcsActionService.Unload();
    }

}
