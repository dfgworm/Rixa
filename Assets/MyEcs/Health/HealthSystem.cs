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

        readonly EcsFilterInject<Inc<ECHealth, ECHealthRegen>> healthRegenFilter = default;

        readonly EcsCustomInject<EventBus> bus = default;

        public void Init(IEcsSystems systems)
        {

        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in healthRegenFilter.Value)
                UpdateRegen(ref healthRegenFilter.Pools.Inc1.Get(i), ref healthRegenFilter.Pools.Inc2.Get(i));
            foreach (int i in bus.Value.GetEventBodies<EVDamage>(out var pool))
                ProcessDamageEvent(ref pool.Get(i));
        }

        void ProcessDamageEvent(ref EVDamage dmg)
        {
            if (!dmg.victim.Unpack(EcsStatic.world, out int victim))
                return;
            if (!healthPool.Value.Has(victim))
                return;
            if (!PlayerBehaviourBase.IsEntityLocalAuthority(victim))
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
    public struct EVDamage : IEventReplicant
    {
        public float amount;
        public EcsPackedEntity victim;
        public EcsPackedEntity dealer;
    }
}