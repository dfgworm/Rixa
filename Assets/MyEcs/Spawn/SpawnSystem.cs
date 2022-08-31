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

        readonly EcsCustomInject<EventBus> bus = default;


        public void Init(IEcsSystems systems)
        {
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<EVSpawn>(out var pool))
                SpawnEntity(i, ref pool.Get(i));
        }
        void SpawnEntity(int eventId, ref EVSpawn ev)
        {
            int entity = world.Value.NewEntity();
            world.Value.GetPool<EMSpawned>().Add(entity).payload = ev.payload;
            ev.spawnedEntity = world.Value.PackEntity(entity);

            ev.payload.UnloadToWorld(world.Value, entity);
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
}