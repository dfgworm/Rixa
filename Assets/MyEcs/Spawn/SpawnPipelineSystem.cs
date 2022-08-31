using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Reflection;

namespace MyEcs.Spawn
{
    public class SpawnPipelineSystem : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<ECSpawnPipeline, EMSpawned>> pipelineFilter = default;

        ISpawnPipeline[] pipelines;

        public void Init(IEcsSystems systems)
        {
            SpawnPipelineIdService.Init();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in pipelineFilter.Value)
                ActivatePipeline(i, ref pipelineFilter.Pools.Inc1.Get(i));
        }
        void ActivatePipeline(int ent, ref ECSpawnPipeline pipeline)
        {
            pipeline.pipeline.Spawn(world.Value, ent);
        }

    }
    public struct ECSpawnPipeline
    {
        public ISpawnPipeline pipeline;
    }
    public class SpawnPipelineBaggage : IBaggage
    {
        public static SpawnPipelineBaggage Get<T>()
            where T : ISpawnPipeline
        {
            var bag = new SpawnPipelineBaggage();
            bag.pipelineId = SpawnPipelineIdService.GetTypeId(typeof(T));
            return bag;
        }
        public byte pipelineId;
        public SpawnPipelineBaggage()
        {
            pipelineId = 0b_1111_1111;
        }
        public void UnloadToWorld(EcsWorld world, int ent) =>
            world.GetPool<ECSpawnPipeline>().Add(ent).pipeline = SpawnPipelineIdService.GetPipeline(pipelineId);
    }


    public interface ISpawnPipeline
    {
        public void Spawn(EcsWorld world, int ent);
        public void Destroy(EcsWorld world, int ent);
    }
}