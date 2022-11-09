using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Spawn
{
    public class DestroySystem : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<ECSpawnPipeline, ECDestroy>> pipelineFilter = default;
        readonly EcsFilterInject<Inc<ECGameObject, ECDestroy>> gameObjectFilter = default;
        readonly EcsPoolInject<ECDestroy> destroyPool = default;

        readonly EcsFilterInject<Inc<ECDestroy>> destroyFilter = default;
        readonly EcsFilterInject<Inc<ECDestroyDelayed>> delayFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in pipelineFilter.Value)
                ActivatePipeline(i, ref pipelineFilter.Pools.Inc1.Get(i));
            foreach (int i in gameObjectFilter.Value)
                DestroyGameObject(i, gameObjectFilter.Pools.Inc1.Get(i).gameObject);
            foreach (int i in destroyFilter.Value)
                DestroyEntity(i);
            foreach (int i in delayFilter.Value)
                UpdateDelay(i, ref delayFilter.Pools.Inc1.Get(i));
        }

        void ActivatePipeline(int ent, ref ECSpawnPipeline pipeline)
        {
            pipeline.pipeline.Destroy(world.Value, ent);
        }
        void DestroyGameObject(int ent, GameObject go)
        {
            GameObject.Destroy(go);
        }
        void DestroyEntity(int entity)
        {
            world.Value.DelEntity(entity);
        }

        void UpdateDelay(int entity, ref ECDestroyDelayed delay)
        {
            delay.time -= Time.deltaTime;
            if (delay.time > 0)
                return;
            if (!destroyPool.Value.Has(entity))
                destroyPool.Value.Add(entity);
        }

    }
    public struct ECDestroy
    {

    }
    public struct ECDestroyDelayed
    {
        public float time;
    }
}