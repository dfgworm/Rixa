using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

[TestFixture]
public class MovementTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsStatic.fixedUpdateSystems
            .Add(new MovementSystem())
            .Add(new MovementObstacleSystem())
            .Inject(EcsStatic.bus)
            .Init()
            ;
    }


    [Test]
    public void Velocity()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var pos = ref EcsStatic.GetPool<ECPosition>().Add(ent);
        ref var vel = ref EcsStatic.GetPool<ECVelocity>().Add(ent);

        vel.velocity = new Vector2(10, 10);

        EcsStatic.fixedUpdateSystems.Run();

        Assert.IsTrue(pos.position2.x > 0);
        Assert.IsTrue(pos.position2.y > 0);
    }

    [Test]
    public void Acceleration()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var vel = ref EcsStatic.GetPool<ECVelocity>().Add(ent);
        ref var acc = ref EcsStatic.GetPool<ECDesiredVelocity>().Add(ent);

        acc.direction = Vector2.one.normalized;
        acc.targetSpeed = 20;
        acc.acceleration = 20;

        EcsStatic.fixedUpdateSystems.Run();

        Assert.IsTrue(vel.velocity.x > 0);
        Assert.IsTrue(vel.velocity.y > 0);

        float numCycles = 4 / Time.fixedDeltaTime;
        for (int i = 1; i < numCycles; i++)
            EcsStatic.fixedUpdateSystems.Run();

        Assert.AreEqual(acc.direction * acc.targetSpeed, vel.velocity);
    }

    [Test]
    public void ObstaclePush()
    {
        int ent = EcsStatic.world.NewEntity();
        ref var pos = ref EcsStatic.GetPool<ECPosition>().Add(ent);
        ref var vel = ref EcsStatic.GetPool<ECVelocity>().Add(ent);
        ref var pushable = ref EcsStatic.GetPool<ECRespectObstacles>().Add(ent);

        int obs = EcsStatic.world.NewEntity();
        ref var pos2 = ref EcsStatic.GetPool<ECPosition>().Add(obs);
        ref var col2 = ref EcsStatic.GetPool<ECCollider>().Add(obs);
        ref var obsComp = ref EcsStatic.GetPool<ECObstacle>().Add(obs);

        pos2.position2 = new Vector2(0.25f, 1);
        col2.type = ColliderType.rectangle;
        col2.size = new Vector2(1, 1);

        var startVel = new Vector2(5, 5);
        vel.velocity = startVel;

        ref var ev = ref EcsStatic.bus.NewEvent<EVCollision>();
        ev.ent1 = EcsStatic.world.PackEntity(ent);
        ev.ent2 = EcsStatic.world.PackEntity(obs);
        EcsStatic.fixedUpdateSystems.Run();

        Assert.IsTrue(pushable.force != Vector2.zero);

        Assert.IsTrue(vel.velocity.y < startVel.y);
        Assert.IsTrue(vel.velocity.y < vel.velocity.x);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

}