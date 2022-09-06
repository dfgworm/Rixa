using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace MyEcs.Physics
{
    public class RotationSystem : IEcsRunSystem
    {

        readonly EcsFilterInject<Inc<ECRotation, ECDestinationRotation>> destinationFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (int i in destinationFilter.Value)
                RotateTo(ref destinationFilter.Pools.Inc1.Get(i), ref destinationFilter.Pools.Inc2.Get(i));
        }

        void RotateTo(ref ECRotation rot, ref ECDestinationRotation dest)
        {
            rot.rotation = Mathf.MoveTowardsAngle(rot.rotation, dest.destination, dest.speed * Time.fixedDeltaTime);
        }

    }
    public struct ECDestinationRotation
    {
        public float destination;
        public float speed;

    }

}