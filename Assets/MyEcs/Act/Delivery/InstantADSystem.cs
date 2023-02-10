using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Acts
{
    //Action Delivery
    public class InstantADSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACInstantDelivery> deliveryPool = "act";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject actWorld = "act";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVUse>(out var pool))
                ProcessActionUse(ref pool.Get(i));

        }

        void ProcessActionUse(ref AEVUse ev)
        {
            if (!ev.act.Unpack(actWorld.Value, out int ac))
                return;
            if (!deliveryPool.Value.Has(ac))
                return;

            if (ev.target.type == ActTargetType.point)
            {
                ref var evHit = ref bus.Value.NewEvent<AEVPointHit>();
                evHit.act = ev.act;
                evHit.point = ev.target.vector;
            }

            if (ev.target.type == ActTargetType.entity)
                if (ev.target.entity.Unpack(world.Value, out int victim))
                {
                    ref var evHit = ref bus.Value.NewEvent<AEVEntityHit>();
                    evHit.act = ev.act;
                    evHit.victim = ev.target.entity;
                }
        }
    }
    public struct ACInstantDelivery
    {
        //public float maxRange; //can make range limitation
    }
}