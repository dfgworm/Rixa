using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;
using MyEcs.Net;

namespace MyEcs.Actions
{
    public class DashActionSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACDash> dashPool = "actions";

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
            if (!dashPool.Value.Has(ac))
                return;
            if (!EcsActionService.TryGetEntity(ac, out int ent))
                return;
            if (!NetOwnershipService.IsEntityLocalAuthority(ent))
                return;

            Vector2 pos = world.Value.GetPool<ECPosition>().Get(ent).position2;
            Vector2 dir;
            if (ev.target.type == ActionTargetType.direction)
                dir = ev.target.vector.normalized;
            else
                dir = (ev.target.vector - pos).normalized;


            ref var dash = ref dashPool.Value.Get(ac);
            world.Value.GetPool<ECVelocity>().Get(ent).velocity = dir*dash.velocity;
            ref var pushMark = ref world.Value.GetPool<ECPushed>().SoftAdd(ent);
            pushMark.duration = dash.range / dash.velocity;
        }

    }
    public struct ACDash
    {
        public float range;
        public float velocity;
    }
}