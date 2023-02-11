using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Health;

namespace MyEcs.Act
{
    public class DamageEffectSystem : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<EventBus> bus = default;

        readonly EcsFilterInject<Inc<AMEntityHit, ACDamage>> entityHitFilter = "act";

        public void Run(IEcsSystems systems)
        {
            foreach (int i in entityHitFilter.Value)
                foreach (var victim in entityHitFilter.Pools.Inc1.Get(i).victims)
                    if (victim.Unpack(world.Value, out int vic))
                        ProcessHit(i, vic, ref entityHitFilter.Pools.Inc2.Get(i));

        }

        void ProcessHit(int ac, int target, ref ACDamage acDmg)
        {
            int ent = ActService.GetEntity(ac);
            ref var dmg = ref bus.Value.NewEvent<EVDamage>();
            dmg.amount = acDmg.amount;
            dmg.dealer = world.Value.PackEntity(ent);
            dmg.victim = world.Value.PackEntity(target);

        }

    }
    public struct ACDamage
    {
        public float amount;
    }
}