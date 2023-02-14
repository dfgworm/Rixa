using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

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
            ref var hp = ref healthPool.Value.Get(victim);
            hp.amount.Current -= dmg.amount;
        }
        void UpdateRegen(ref ECHealth hp, ref ECHealthRegen regen)
        {
            hp.amount.Current += regen.rate*Time.deltaTime;
        }

    }
    public struct ECHealth
    {
        public FloatLimited amount;
    }
    public struct ECHealthRegen
    {
        public float rate;
    }
    public struct EVDamage : IEventReplicant
    {
        public float amount;
        public EcsPackedEntity victim;
        public EcsPackedEntity dealer;
    }
}