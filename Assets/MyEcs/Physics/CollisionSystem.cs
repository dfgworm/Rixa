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
        readonly EcsPoolInject<ECCollisionHashFilter> filterPool = default;

        readonly EcsFilterInject<Inc<ECPosition, ECCollider>> colliderFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var pair in colliderFilter.Value.EnumPairsWithSelf())
                CheckCollision(pair.i, pair.j);
        }

        void CheckCollision(int ent1, int ent2)
        {
            if (!CheckFilters(ent1, ent2))
                return;

            ref var pos1 = ref colliderFilter.Pools.Inc1.Get(ent1);
            ref var col1 = ref colliderFilter.Pools.Inc2.Get(ent1);
            ref var pos2 = ref colliderFilter.Pools.Inc1.Get(ent2);
            ref var col2 = ref colliderFilter.Pools.Inc2.Get(ent2);

            if (CollisionSolver.Solve(ref pos1, ref col1, ref pos2, ref col2))
                RegisterCollision(ent1, ent2);
        }
        bool CheckFilters(int ent1, int ent2)
        {
            return CheckFiltersOf(ent1, ent2) && CheckFiltersOf(ent2, ent1);
        }
        bool CheckFiltersOf(int ent, int toFilter)
        {
            if (!filterPool.Value.Has(ent))
                return true;
            var set = filterPool.Value.Get(ent).filter;
            if (set.Contains(toFilter))
                return false;
            return true;
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
    public struct ECCollisionHashFilter : IEcsAutoReset<ECCollisionHashFilter>
    {
        public HashSet<int> filter;

        public void AutoReset(ref ECCollisionHashFilter c)
        {
            if (c.filter == null)
                c.filter = new HashSet<int>(8);
            else
                c.filter.Clear();
        }
    }

    public struct EVCollision : IEventReplicant
    {
        public EcsPackedEntity ent1;
        public EcsPackedEntity ent2;
        public bool Unpack(out int e1, out int e2) =>
            Unpack(EcsStatic.world, out e1, out e2);
        public bool Unpack(EcsWorld world, out int e1, out int e2)
        {
            e2 = -1;
            if (!ent1.Unpack(world, out e1))
                return false;
            if (!ent2.Unpack(world, out e2))
                return false;
            return true;
        }
    }
}