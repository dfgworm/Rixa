using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Health;

namespace MyEcs.Acts
{
    public class HealEffectSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACHeal> healPool = "act";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject actWorld = "act";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVEntityHit>(out var pool))
                ProcessActUse(ref pool.Get(i));

        }

        void ProcessActUse(ref AEVEntityHit ev)
        {
            if (!ev.act.Unpack(actWorld.Value, out int ac))
                return;
            if (!healPool.Value.Has(ac))
                return;
            if (!ActService.TryGetEntity(ac, out int ent))
                return;

            if (!ev.victim.Unpack(world.Value, out int target))
                return;
            if (!world.Value.GetPool<ECHealth>().Has(target))
                return;
            ref var heal = ref healPool.Value.Get(ac);
            world.Value.GetPool<ECHealth>().Get(target).Current += heal.amount; //should this be done via event like EVDamage?

        }

    }
    public struct ACHeal
    {
        public float amount;
    }
}