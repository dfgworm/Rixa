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
    public class HealEffectSystem : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsFilterInject<Inc<AMEntityHit, ACHeal>> entityHitFilter = "act";

        public void Run(IEcsSystems systems)
        {
            foreach (int i in entityHitFilter.Value)
                foreach (var victim in entityHitFilter.Pools.Inc1.Get(i).victims)
                    if (victim.Unpack(world.Value, out int vic))
                        ProcessHit(i, vic, ref entityHitFilter.Pools.Inc2.Get(i));

        }

        void ProcessHit(int ac, int target, ref ACHeal heal)
        {
            if (!world.Value.GetPool<ECHealth>().Has(target))
                return;
            world.Value.GetPool<ECHealth>().Get(target).amount.Current += heal.amount; //should this be done via event like EVDamage?

        }

    }
    public struct ACHeal
    {
        public float amount;
    }
}