using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Spawn
{
    public class SpawnSystem : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<ECGameObject, EMSpawned>> gameObjectFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;


        public void Init(IEcsSystems systems)
        {
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<EVSpawn>(out var pool))
                SpawnEntity(i, ref pool.Get(i));
            foreach (int i in gameObjectFilter.Value)
                Augment(i, gameObjectFilter.Pools.Inc1.Get(i).gameObject);
        }
        void SpawnEntity(int eventId, ref EVSpawn ev)
        {
            int entity = world.Value.NewEntity();
            world.Value.GetPool<EMSpawned>().Add(entity).payload = ev.payload;
            ev.spawnedEntity = world.Value.PackEntity(entity);

            ev.payload.UnloadToWorld(world.Value, entity);
        }

        void Augment(int entity, GameObject go)
        {

            ISpawnEarlyAugment[] earlyAugmenters = go.GetComponents<ISpawnEarlyAugment>();
            foreach (var aug in earlyAugmenters)
                aug.EarlyAugment(entity);

            ISpawnAugment[] augmenters = go.GetComponents<ISpawnAugment>();
            foreach (var aug in augmenters)
                aug.SpawnAugment(entity);

            ISpawnLateAugment[] lateAugmenters = go.GetComponents<ISpawnLateAugment>();
            foreach (var aug in lateAugmenters)
                aug.LateAugment(entity);
        }
    }
    public struct EVSpawn : IEventReplicant
    {
        public EcsPackedEntity spawnedEntity;
        public BaggagePayload payload;
    }
    public struct EMSpawned
    {
        public BaggagePayload payload;
    }
    public interface ISpawnEarlyAugment
    {
        public void EarlyAugment(int ent);
    }
    public interface ISpawnAugment
    {
        public void SpawnAugment(int ent);
    }
    public interface ISpawnLateAugment
    {
        public void LateAugment(int ent);
    }
}