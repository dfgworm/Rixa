using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Physics
{
    public class CollisionSystem : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<ECPosition, ECCollider>> colliderFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var pair in colliderFilter.Value.EnumPairsWithSelf())
                CheckCollision(pair.i, pair.j);
        }

        void CheckCollision(int ent1, int ent2)
        {
            ref var pos1 = ref colliderFilter.Pools.Inc1.Get(ent1);
            ref var col1 = ref colliderFilter.Pools.Inc2.Get(ent1);
            ref var pos2 = ref colliderFilter.Pools.Inc1.Get(ent2);
            ref var col2 = ref colliderFilter.Pools.Inc2.Get(ent2);

            if (CollisionSolver.Solve(ref pos1, ref col1, ref pos2, ref col2))
                RegisterCollision(ent1, ent2);
        }
        void RegisterCollision(int ent1, int ent2)
        {
            ref var ev = ref bus.Value.NewEvent<EVCollision>();
            ev.ent1 = world.Value.PackEntity(ent1);
            ev.ent2 = world.Value.PackEntity(ent2);
        }

    }
    public struct ECCollider
    {
        public ColliderType type;
        public Vector2 size;
    }

    public struct EVCollision : IEventReplicant
    {
        public EcsPackedEntity ent1;
        public EcsPackedEntity ent2;
    }
}