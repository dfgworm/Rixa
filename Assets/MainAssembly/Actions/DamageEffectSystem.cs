using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Health;

namespace MyEcs.Actions
{
    public class DamageEffectSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACDamage> damagePool = "actions";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVEntityHit>(out var pool))
                ProcessActionUse(ref pool.Get(i));

        }

        void ProcessActionUse(ref AEVEntityHit ev)
        {
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!damagePool.Value.Has(ac))
                return;
            if (!EcsActionService.TryGetEntity(ac, out int ent))
                return;

            if (!ev.victim.Unpack(world.Value, out int target))
                return;
            if (!world.Value.GetPool<ECHealth>().Has(target))
                return;
            //perhaps i need a net authority check here
            ref var acDmg = ref damagePool.Value.Get(ac);
            ref var dmg = ref bus.Value.NewEvent<EVDamage>();
            dmg.amount = acDmg.amount;
            dmg.dealer = world.Value.PackEntity(ent);
            dmg.victim = ev.victim;

        }

    }
    public struct ACDamage
    {
        public float amount;
    }
}