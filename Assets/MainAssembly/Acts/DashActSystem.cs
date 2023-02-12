using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

namespace MyEcs.Act
{
    public class DashActSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACDash> dashPool = "act";


        readonly EcsFilterInject<Inc<AMUsed, ACDash>> useFilter = "act";


        public void Run(IEcsSystems systems)
        {

            foreach (int i in useFilter.Value)
                foreach (var usage in useFilter.Pools.Inc1.Get(i).usages)
                    ProcessActUse(i, usage);
        }

        void ProcessActUse(int act, ActUsageContainer usage)
        {
            int ent = ActService.GetEntity(act);
            Vector2 pos = EcsStatic.GetPool<ECPosition>().Get(ent).position2;
            Vector2 dir;
            if (usage.targetType == ActTargetType.direction)
                dir = usage.vector.normalized;
            else
                dir = (usage.vector - pos).normalized;


            ref var dash = ref dashPool.Value.Get(act);
            ref var pushMark = ref EcsStatic.GetPool<ECPushed>().SafeAdd(ent);
            pushMark.duration = dash.range / dash.velocity;
            pushMark.velocity = dir*dash.velocity;
            pushMark.acceleration = dash.maxForce;
            if (EcsStatic.GetPool<ECVelocity>().Has(ent))
                EcsStatic.GetPool<ECVelocity>().Get(ent).velocity = pushMark.velocity;
        }

    }
    public struct ACDash
    {
        public float range;
        public float velocity;
        public float maxForce;
    }
}