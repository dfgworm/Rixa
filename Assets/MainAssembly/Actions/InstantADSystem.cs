using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;
using MyEcs.Physics;

namespace MyEcs.Actions
{
    //Action Delivery
    public class InstantADSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACInstantDelivery> deliveryPool = "actions";

        readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVUse>(out var pool))
                ProcessActionUse(ref pool.Get(i));

        }

        void ProcessActionUse(ref AEVUse ev)
        {
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!deliveryPool.Value.Has(ac))
                return;

            if (ev.target.type == ActionTargetType.point)
            {
                ref var evHit = ref bus.Value.NewEvent<AEVPointHit>();
                evHit.action = ev.action;
                evHit.point = ev.target.vector;
            }

            if (ev.target.type == ActionTargetType.entity)
                if (ev.target.entity.Unpack(world.Value, out int victim))
                {
                    ref var evHit = ref bus.Value.NewEvent<AEVEntityHit>();
                    evHit.action = ev.action;
                    evHit.victim = ev.target.entity;
                }
        }
    }
    public struct ACInstantDelivery
    {
        //public float maxRange; //can make range limitation
    }
}