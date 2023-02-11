using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Act
{
    //Action Delivery
    public class InstantADSystem : IEcsRunSystem
    {


        readonly EcsFilterInject<Inc<AMUsed, ACInstantDelivery>> useFilter = "act";

        public void Run(IEcsSystems systems)
        {

            foreach (int i in useFilter.Value)
                foreach (var usage in useFilter.Pools.Inc1.Get(i).usages)
                    ProcessActUse(i, usage);

        }

        void ProcessActUse(int act, ActUsageContainer usage)
        {
            if (usage.targetType == ActTargetType.point)
                ActService.GetPool<AMPointHit>().SafeAdd(act).points.Add(usage.vector);
            if (usage.targetType == ActTargetType.entity)
                ActService.GetPool<AMEntityHit>().SafeAdd(act).victims.Add(usage.entity);
        }
    }
    public struct ACInstantDelivery
    {
        //public float maxRange; //can make range limitation
    }
}