using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace MyEcs.Physics
{
    public class MovementSystem : IEcsRunSystem
    {

        readonly EcsFilterInject<Inc<ECVelocity, ECTargetVelocity>, Exc<ECPushed>> accelerationFilter = default;
        readonly EcsFilterInject<Inc<ECPosition, ECVelocity>> moveFilter = default;
        readonly EcsFilterInject<Inc<ECPushed, ECVelocity>> pushFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (int i in accelerationFilter.Value)
                Accelerate(ref accelerationFilter.Pools.Inc1.Get(i), ref accelerationFilter.Pools.Inc2.Get(i));
            foreach (int i in pushFilter.Value)
                Push(i, ref pushFilter.Pools.Inc1.Get(i), ref pushFilter.Pools.Inc2.Get(i));
            foreach (int i in moveFilter.Value)
                Move(ref moveFilter.Pools.Inc1.Get(i), ref moveFilter.Pools.Inc2.Get(i));

        }

        void Accelerate(ref ECVelocity vel, ref ECTargetVelocity accel)
        {
            vel.velocity = Vector2.MoveTowards(vel.velocity, accel.direction * accel.targetSpeed, accel.acceleration*Time.fixedDeltaTime);
        }
        void Push(int ent, ref ECPushed push, ref ECVelocity vel)
        {
            push.duration -= Time.fixedDeltaTime;
            if (push.duration <= 0)
                pushFilter.Pools.Inc1.Del(ent);
            else
                vel.velocity = Vector2.MoveTowards(vel.velocity, push.velocity, push.acceleration * Time.fixedDeltaTime);
        }
        void Move(ref ECPosition pos, ref ECVelocity vel)
        {
            if (!vel.IsMoving)
                return;
            pos.position2 += vel.velocity * Time.fixedDeltaTime;
        }

    }
    public struct ECTargetVelocity
    {
        public Vector2 direction;
        public float targetSpeed;
        public float acceleration;

        public bool IsMoving => direction != Vector2.zero && Math.Abs(targetSpeed) > 1e-05;
    }
    public struct ECVelocity
    {
        public Vector2 velocity;
        public bool IsMoving => velocity != Vector2.zero;
    }
    public struct ECPushed
    {
        public float duration;
        public Vector2 velocity;
        public float acceleration;
    }
}