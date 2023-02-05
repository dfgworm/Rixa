using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Spawn;

namespace MyEcs.Health
{
    public static class HealthPipe
    {

        public static void BuildHealth(EcsWorld world, int ent, float max, float regen = 0)
        {
            ref var hp = ref world.GetPool<ECHealth>().Add(ent);
            hp.max = max;
            hp.Percent = 1;
            if (regen > 0)
                world.GetPool<ECHealthRegen>().Add(ent).regen = regen;
            ref var hpDisp = ref world.GetPool<ECHealthDisplay>().Add(ent);
            hpDisp.Init();
            hpDisp.controller.shift = new Vector3(0,3,0);
            world.GetPool<EMSpawned>().Get(ent).payload.EnsureBaggageAndUnload<HealthBaggage>(world, ent);
        }
    }

    public class HealthBaggage : IUpdatableBaggage
    {
        public float health;
        public bool IsUpToDate(EcsWorld world, int ent)
        {
            return world.GetPool<ECHealth>().Get(ent).Current == health;
        }
        public void LoadToBaggage(EcsWorld world, int ent)
        {
            var pool = world.GetPool<ECHealth>();
            health = pool.Get(ent).Current;
        }
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECHealth>().Get(ent).Current = health;
        }
    }
}