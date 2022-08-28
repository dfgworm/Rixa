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

        readonly EcsFilterInject<Inc<ECGameObject, ECDestroy>> gameObjectFilter = default;
        readonly EcsPoolInject<ECDestroy> destroyPool = default;
        readonly EcsPoolInject<ECDestroyDelayed> delayedPool = default;

        readonly EcsFilterInject<Inc<ECDestroy>> destroyFilter = default;
        readonly EcsFilterInject<Inc<ECDestroyDelayed>> delayFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in gameObjectFilter.Value)
                Augment(i, gameObjectFilter.Pools.Inc1.Get(i).gameObject);
            foreach (int i in destroyFilter.Value)
                DestroyEntity(i);
            foreach (int i in delayFilter.Value)
                UpdateDelay(i, ref delayFilter.Pools.Inc1.Get(i));
        }

        void Augment(int entity, GameObject go)
        {
            IDestroyAugment[] augments = go.GetComponents<IDestroyAugment>();
            foreach (var aug in augments)
                aug.DestroyAugment(entity);
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
            delayedPool.Value.Del(entity);
        }

    }
    public struct ECDestroy
    {

    }
    public struct ECDestroyDelayed
    {
        public float time;
    }
    public interface IDestroyAugment
    {
        public void DestroyAugment(int ent);
    }
}