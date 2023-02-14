using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Act
{
    public class AmmoLimitationSystem : IEcsRunSystem
    {
        readonly EcsFilterInject<Inc<ACAmmo, ACAmmoRegen>> regenFilter = "act";
        readonly EcsFilterInject<Inc<AMUsed, ACAmmo>> useFilter = "act";

        public void Run(IEcsSystems systems)
        {
            foreach (int i in regenFilter.Value)
                UpdateRegen(ref regenFilter.Pools.Inc1.Get(i), regenFilter.Pools.Inc2.Get(i));
            foreach (int i in useFilter.Value)
                ProcessActUse(i, ref useFilter.Pools.Inc1.Get(i), ref useFilter.Pools.Inc2.Get(i));

        }
        void UpdateRegen(ref ACAmmo ammo, ACAmmoRegen regen)
        {
            ammo.amount.Current += regen.rate*Time.deltaTime;
        }
        void ProcessActUse(int ac, ref AMUsed usage, ref ACAmmo ammo)
            //ActChannelingSystem both consumes and produces AMUsed marks, which conflicts with this system
            // as of now, channel-starting acts are not filtered
        {
            int count = usage.usages.Count;
            int available = Mathf.FloorToInt(ammo.amount.Current);
            if (count > available)
            {
                usage.usages.RemoveRange(available, count - available);
                count = available;
            }
            ammo.amount.Current -= count;
        }

    }
    public struct ACAmmo
    {
        public FloatLimited amount;

    }
    public struct ACAmmoRegen
    {
        public float rate;
        public float Cooldown { get => 1 / rate; set => rate = 1 / value; }
    }
}