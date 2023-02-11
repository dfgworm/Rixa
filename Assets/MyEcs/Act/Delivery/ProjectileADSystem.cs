using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

namespace MyEcs.Act
{
    //Action Delivery
    public class ProjectileADSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ECProjectile> projPool = default;

        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<EventBus> bus = default;

        readonly EcsFilterInject<Inc<AMUsed, ACProjectileDelivery>> useFilter = "act";


        public void Run(IEcsSystems systems)
        {
            foreach (int i in useFilter.Value)
                foreach(var usage in useFilter.Pools.Inc1.Get(i).usages)
                    ProcessActUse(i, usage);
            foreach (int i in bus.Value.GetEventBodies<EVCollision>(out var pool))
                ProcessCollision(ref pool.Get(i));

        }

        void ProcessActUse(int ac, ActUsageContainer usage)
        {
            int ent = ActService.GetEntity(ac);

            Vector2 pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
            Vector2 dir;
            if (usage.targetType == ActTargetType.direction)
                dir = usage.vector.normalized;
            else
                dir = (usage.vector - pos).normalized;

            ProjectileSP.Spawn(ac, pos, dir);
        }

        void ProcessCollision(ref EVCollision collision)
        {
            if (!collision.Unpack(out int ent1, out int ent2))
                return;
            if (projPool.Value.Has(ent1))
                ProcessProjectileHit(ent1, ent2);
            if (projPool.Value.Has(ent2))
                ProcessProjectileHit(ent2, ent1);
        }
        void ProcessProjectileHit(int projEnt, int victim)
        {
            ref var proj = ref projPool.Value.Get(projEnt);
            if (!proj.sourceAct.Unpack(ActService.world, out int act))
                return;
            ActService.GetPool<AMPointHit>().SafeAdd(act).points.Add(world.Value.GetPool<ECPosition>().Get(projEnt).position2);
            ActService.GetPool<AMEntityHit>().SafeAdd(act).victims.Add(world.Value.PackEntity(victim));
            //self destruction is handled by ECSelfDestructOnCollision attached to projectile
        }
    }
    public struct ECProjectile
    {
        public EcsPackedEntity sourceAct;
    }
    public struct ACProjectileDelivery
    {
        public bool selfDestruct;
        public float velocity;
        public float lifetime;
    }
}