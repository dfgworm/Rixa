using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

namespace MyEcs.Acts
{
    //Action Delivery
    public class ProjectileADSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ECProjectile> projPool = default;
        readonly EcsPoolInject<ACProjectileDelivery> projLaunchPool = "act";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject actWorld = "act";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVUse>(out var pool))
                ProcessActionUse(ref pool.Get(i));
            foreach (int i in bus.Value.GetEventBodies<EVCollision>(out var pool))
                ProcessCollision(ref pool.Get(i));

        }

        void ProcessActionUse(ref AEVUse ev)
        {
            if (!ev.act.Unpack(actWorld.Value, out int ac))
                return;
            if (!projLaunchPool.Value.Has(ac))
                return;
            if (!ActService.TryGetEntity(ac, out int ent))
                return;

            Vector2 pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
            Vector2 dir;
            if (ev.target.type == ActTargetType.direction)
                dir = ev.target.vector.normalized;
            else
                dir = (ev.target.vector - pos).normalized;

            ProjectileSP.Spawn(ent, ac, dir);
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
            ref var entityHit = ref bus.Value.NewEvent<AEVEntityHit>();
            entityHit.act = proj.sourceAct;
            entityHit.victim = world.Value.PackEntity(victim);

            ref var pointHit = ref bus.Value.NewEvent<AEVPointHit>();
            pointHit.act = proj.sourceAct;
            pointHit.point = world.Value.GetPool<ECPosition>().Get(projEnt).position2;
        }
    }
    public struct ECProjectile
    {
        public EcsPackedEntity ownerEntity;
        public EcsPackedEntity sourceAct;
    }
    public struct ACProjectileDelivery
    {
        public bool selfDestruct;
        public float velocity;
        public float lifetime;
    }
}