using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;
using MyEcs.Physics;

namespace MyEcs.Actions
{
    //Action Delivery
    public class ProjectileADSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ECProjectile> projPool = default;
        readonly EcsPoolInject<ACProjectileDelivery> projLaunchPool = "actions";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

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
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!projLaunchPool.Value.Has(ac))
                return;
            if (!EcsActionService.TryGetEntity(ac, out int ent))
                return;

            Vector2 pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
            Vector2 dir;
            if (ev.target.type == ActionTargetType.direction)
                dir = ev.target.vector.normalized;
            else
                dir = (ev.target.vector - pos).normalized;

            ref var proj = ref projLaunchPool.Value.Get(ac);
            ref var spawnEv = ref bus.Value.NewEvent<EVSpawn>();
            spawnEv.Payload
                .Add(SpawnPipelineBaggage.Get<ProjectileSP>())
                .Add(new PositionBaggage { position = pos })
                .Add(new VelocityBaggage { velocity = dir * proj.velocity })
                .Add(new LifetimeBaggage { lifetime = proj.lifetime })
                .Add(new ProjectileBaggage
                {
                    ownerEntity = world.Value.PackEntity(ent),
                    sourceAction = acWorld.Value.PackEntity(ac),
                });
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
            entityHit.action = proj.sourceAction;
            entityHit.victim = world.Value.PackEntity(victim);

            ref var pointHit = ref bus.Value.NewEvent<AEVPointHit>();
            pointHit.action = proj.sourceAction;
            pointHit.point = world.Value.GetPool<ECPosition>().Get(projEnt).position2;
        }
    }
    public struct ECProjectile
    {
        public EcsPackedEntity ownerEntity;
        public EcsPackedEntity sourceAction;
    }
    public struct ACProjectileDelivery
    {
        public bool selfDestruct;
        public float velocity;
        public float lifetime;
    }
}