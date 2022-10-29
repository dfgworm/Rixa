using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Net;

namespace MyEcs.Health
{
    public class HealthSystem : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsPoolInject<ECHealth> healthPool = default;
        readonly EcsPoolInject<ECTouchDamage> touchDamagePool = default;

        readonly EcsFilterInject<Inc<ECHealth, ECHealthRegen>> healthRegenFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;
        readonly EcsWorldInject world = default;

        public void Init(IEcsSystems systems)
        {

        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<EVCollision>(out var pool))
                ProcessCollision(ref pool.Get(i));

            foreach (int i in healthRegenFilter.Value)
                UpdateRegen(ref healthRegenFilter.Pools.Inc1.Get(i), ref healthRegenFilter.Pools.Inc2.Get(i));
            foreach (int i in bus.Value.GetEventBodies<EVDamage>(out var pool))
                ProcessDamageEvent(ref pool.Get(i));

        }
        void ProcessCollision(ref EVCollision collision)
        {
            if (!collision.ent1.Unpack(world.Value, out int ent1))
                return;
            if (!collision.ent2.Unpack(world.Value, out int ent2))
                return;
            if (touchDamagePool.Value.Has(ent1))
                MakeTouchDamage(ent1, ent2);
            if (touchDamagePool.Value.Has(ent2))
                MakeTouchDamage(ent2, ent1);
        }
        void MakeTouchDamage(int dealer, int victim)
        {
            var touchDamage = touchDamagePool.Value.Get(dealer);
            ref var dmg = ref bus.Value.NewEvent<EVDamage>();
            dmg.dealer = world.Value.PackEntity(dealer);
            dmg.victim = world.Value.PackEntity(victim);
            dmg.amount = touchDamage.dps*Time.deltaTime;
        }

        void ProcessDamageEvent(ref EVDamage dmg)
        {
            if (!dmg.victim.Unpack(EcsStatic.world, out int victim))
                return;
            if (!healthPool.Value.Has(victim))
                return;
            if (!ECNetOwner.IsEntityLocalAuthority(victim))
                return;
            ref var hp = ref healthPool.Value.Get(victim);
            hp.Current -= dmg.amount;
        }
        void UpdateRegen(ref ECHealth hp, ref ECHealthRegen regen)
        {
            hp.Current += regen.regen*Time.deltaTime;
        }

    }
    public struct ECHealth
    {
        float _current;
        public float max;
        public float Current
        {
            get => _current;
            set => _current = Mathf.Clamp(value, 0, max);
        }

        public float Percent
        {
            get => _current / max;
            set => _current = Mathf.Clamp01(value) * max;
        }
    }
    public struct ECHealthRegen
    {
        public float regen;
    }
    public struct ECTouchDamage
    {
        public float dps;
    }
    public struct EVDamage : IEventReplicant
    {
        public float amount;
        public EcsPackedEntity victim;
        public EcsPackedEntity dealer;
    }
}