using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;

[TestFixture]
public class SpawnPipelineTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
        EcsStatic.updateSystems.Add(new DestroySystem()).Add(new SpawnSystem()).Add(new SpawnPipelineSystem())
            .Inject(EcsStatic.bus).Init();
    }

    [Test]
    public void PipelineSpawnDestroyActivation()
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.payload = new BaggagePayload().Add(SpawnPipelineBaggage.Get<TestPipeline>());

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(ev.spawnedEntity.Unpack(EcsStatic.world, out int ent));
        Assert.IsTrue(EcsStatic.GetPool<ECSpawnPipeline>().Has(ent));
        Assert.AreSame(EcsStatic.GetPool<ECSpawnPipeline>().Get(ent).pipeline, SpawnPipelineIdService.GetPipeline<TestPipeline>());

        int[] _ = null;
        int c = EcsStatic.world.GetAllEntities(ref _);
        Assert.AreEqual(1, c);


        Assert.IsTrue(EcsStatic.GetPool<TestComp>().Has(ent));

        EcsStatic.GetPool<ECDestroy>().Add(ent);

        EcsStatic.updateSystems.Run();

        Assert.IsTrue(EcsStatic.bus.HasEventSingleton<TestEventSingleton>());
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
    public struct TestEventSingleton : IEventSingleton
    {
    }
    public struct TestComp
    {
    }
    public class DummyPipeline1 : ISpawnPipeline
    {
        public void Destroy(EcsWorld world, int ent) => throw new System.NotImplementedException();

        public void Spawn(EcsWorld world, int ent) => throw new System.NotImplementedException();
    }
    public class DummyPipeline2 : ISpawnPipeline
    {
        public void Destroy(EcsWorld world, int ent) => throw new System.NotImplementedException();

        public void Spawn(EcsWorld world, int ent) => throw new System.NotImplementedException();
    }
    public class TestPipeline : ISpawnPipeline
    {
        public void Spawn(EcsWorld world, int ent)
        {
            world.GetPool<TestComp>().Add(ent);
        }
        public void Destroy(EcsWorld world, int ent)
        {
            EcsStatic.bus.NewEventSingleton<TestEventSingleton>();
        }
    }
    public class TestPipeline2 : ISpawnPipeline
    {
        public void Destroy(EcsWorld world, int ent) => throw new System.NotImplementedException();

        public void Spawn(EcsWorld world, int ent) => throw new System.NotImplementedException();
    }

    public class DummyPipeline3 : ISpawnPipeline
    {
        public void Destroy(EcsWorld world, int ent) => throw new System.NotImplementedException();

        public void Spawn(EcsWorld world, int ent) => throw new System.NotImplementedException();
    }
}
