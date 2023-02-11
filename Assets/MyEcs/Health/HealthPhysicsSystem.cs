using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

namespace MyEcs.Health
{
    public class HealthPhysicsSystem : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsPoolInject<ECDestroy> destroyPool = default;
        readonly EcsPoolInject<ECDamageOnCollision> damageOnColPool = default;
        readonly EcsPoolInject<ECTouchDamage> touchDamagePool = default;


        readonly EcsCustomInject<EventBus> bus = default;
        readonly EcsWorldInject world = default;

        public void Init(IEcsSystems systems)
        {

        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<EVCollision>(out var pool))
                ProcessCollision(ref pool.Get(i));

        }
        void ProcessCollision(ref EVCollision collision)
        {
            if (!collision.Unpack(out int ent1, out int ent2))
                return;

            if (touchDamagePool.Value.Has(ent1))
                MakeTouchDamage(ent1, ent2);
            if (touchDamagePool.Value.Has(ent2))
                MakeTouchDamage(ent2, ent1);

            ProcessDamageOnCollision(ent1, ent2);
            ProcessDamageOnCollision(ent2, ent1);
            ProcessLimitedCollision(ent1, ent2);
            ProcessLimitedCollision(ent2, ent1);
            ProcessSelfDestruct(ent1);
            ProcessSelfDestruct(ent2);
        }
        void MakeTouchDamage(int dealer, int victim)
        {
            var touchDamage = touchDamagePool.Value.Get(dealer);
            ref var dmg = ref bus.Value.NewEvent<EVDamage>();
            dmg.dealer = world.Value.PackEntity(dealer);
            dmg.victim = world.Value.PackEntity(victim);
            dmg.amount = touchDamage.dps*Time.fixedDeltaTime;
        }

        void ProcessDamageOnCollision(int ent1, int ent2)
        {
            if (!damageOnColPool.Value.Has(ent1) || destroyPool.Value.Has(ent1))
                return;
            float dmg = damageOnColPool.Value.Get(ent1).damage;
            ref var ev = ref bus.Value.NewEvent<EVDamage>();
            ev.amount = dmg;
            ev.dealer = world.Value.PackEntity(ent1);
            ev.victim = world.Value.PackEntity(ent2);
        }
        void ProcessLimitedCollision(int ent1, int ent2)
        {
            if (!world.Value.GetPool<ECLimitedCollision>().Has(ent1))
                return;
            var set = world.Value.GetPool<ECCollisionHashFilter>().SafeAdd(ent1).filter;
            set.Add(ent2);
        }
        void ProcessSelfDestruct(int ent)
        {
            if (!world.Value.GetPool<ECSelfDestructOnCollision>().Has(ent))
                return;
            destroyPool.Value.SafeAdd(ent);
        }
    }
    public struct ECTouchDamage
    {
        public float dps;
    }
    public struct ECDamageOnCollision
    {
        public float damage;
    }
    public struct ECSelfDestructOnCollision
    {
    }
    public struct ECLimitedCollision
    {
    }
}