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


    [Test]
    public void CollisionEventTest()
    {
        EcsStatic.fixedUpdateSystems
            .Add(EcsStatic.bus.GetDestroyEventsSystem().IncReplicant<EVCollision>())
            .Add(new CollisionSystem())
            .Inject(EcsStatic.bus).Init();

        int ent1 = EcsStatic.world.NewEntity();
        ref var pos1 = ref EcsStatic.GetPool<ECPosition>().Add(ent1);
        ref var col1 = ref EcsStatic.GetPool<ECCollider>().Add(ent1);
        col1.type = ColliderType.rectangle;
        col1.size = new Vector2(1, 1);

        int ent2 = EcsStatic.world.NewEntity();
        ref var pos2 = ref EcsStatic.GetPool<ECPosition>().Add(ent2);
        ref var col2 = ref EcsStatic.GetPool<ECCollider>().Add(ent2);
        col2.type = ColliderType.rectangle;
        col2.size = new Vector2(1, 1);

        pos2.position2 = new Vector2(1.5f, 1.5f);
        EcsStatic.fixedUpdateSystems.Run();
        Assert.AreEqual(0, CountCollisions());

        pos2.position2 = new Vector2(0.5f, 0.5f);
        EcsStatic.fixedUpdateSystems.Run();
        Assert.AreEqual(1, CountCollisions());

        int ent3 = EcsStatic.world.NewEntity();
        ref var pos3 = ref EcsStatic.GetPool<ECPosition>().Add(ent3);
        ref var col3 = ref EcsStatic.GetPool<ECCollider>().Add(ent3);
        col3.type = ColliderType.rectangle;
        col3.size = new Vector2(1, 1);

        pos3.position2 = new Vector2(4f, 0f);
        EcsStatic.fixedUpdateSystems.Run();
        Assert.AreEqual(1, CountCollisions());

        pos3.position2 = new Vector2(1.2f, 1.2f);
        EcsStatic.fixedUpdateSystems.Run();
        Assert.AreEqual(2, CountCollisions());

        pos3.position2 = new Vector2(0.5f, 0);
        EcsStatic.fixedUpdateSystems.Run();
        Assert.AreEqual(3, CountCollisions());
    }
    int CountCollisions()
    {
        int c = 0;
        foreach (int _ in EcsStatic.bus.GetEventBodies<EVCollision>(out var _))
            c++;
        return c;
    }

    [Test]
    public void CollisionSolverTest()
    {
        int ent1 = EcsStatic.world.NewEntity();
        var pos1 = EcsStatic.GetPool<ECPosition>().Add(ent1);
        var col1 = EcsStatic.GetPool<ECCollider>().Add(ent1);
        col1.size = new Vector2(1, 1);

        int ent2 = EcsStatic.world.NewEntity();
        var pos2 = EcsStatic.GetPool<ECPosition>().Add(ent2);
        var col2 = EcsStatic.GetPool<ECCollider>().Add(ent2);
        col2.size = new Vector2(1, 1);

        for (int i = 0; i < 500; i++)
        {
            //rect
            pos2.position2 = new Vector2(Random.Range(-0.99f, 0.99f), Random.Range(-0.99f, 0.99f));
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            pos2.position2 = Random.insideUnitCircle * 1.49f;
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            pos2.position2 = new Vector2(Random.Range(-0.49f, 0.49f), Random.Range(-0.49f, 0.49f));
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.point, pos2.position2, col2.size));
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.point, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            pos2.position2 = new Vector2(Random.Range(1.01f, 4), Random.Range(-1.01f, -4));
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            pos2.position2 = Random.insideUnitCircle.normalized * (2 + Random.value);
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            pos2.position2 = new Vector2(Random.Range(0.51f, 3), Random.Range(-0.51f, -3));
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.rectangle, pos1.position2, col1.size, ColliderType.point, pos2.position2, col2.size));
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.point, pos1.position2, col1.size, ColliderType.rectangle, pos2.position2, col2.size));

            //circle
            pos2.position2 = Random.insideUnitCircle * 1.99f;
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));

            pos2.position2 = Random.insideUnitCircle * 0.99f;
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.point, pos2.position2, col2.size));
            Assert.IsTrue(CollisionSolver.Solve(ColliderType.point, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));

            pos2.position2 = Random.insideUnitCircle.normalized * (2.01f + Random.value);
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));

            pos2.position2 = Random.insideUnitCircle.normalized * (1.01f + Random.value);
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.circle, pos1.position2, col1.size, ColliderType.point, pos2.position2, col2.size));
            Assert.IsFalse(CollisionSolver.Solve(ColliderType.point, pos1.position2, col1.size, ColliderType.circle, pos2.position2, col2.size));
        }
    }



    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }

}