using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

namespace MyEcs.Health
{
    public static class HealthPipe
    {

        public static void BuildHealth(int ent, float max, float regen = 0)
        {
            ref var hp = ref EcsStatic.GetPool<ECHealth>().Add(ent);
            hp.max = max;
            hp.Percent = 1;
            if (regen > 0)
                EcsStatic.GetPool<ECHealthRegen>().Add(ent).rate = regen;
            ref var hpDisp = ref EcsStatic.GetPool<ECHealthDisplay>().Add(ent);
            hpDisp.Init();
            hpDisp.controller.shift = new Vector3(0, 3, 0);
        }
    }
}