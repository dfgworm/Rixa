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

        readonly EcsFilterInject<Inc<ECPosition, ECVelocity>> velFilter = default;
        readonly EcsFilterInject<Inc<ECVelocity, ECAcceleration>> moveFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (int i in velFilter.Value)
                Move(ref velFilter.Pools.Inc1.Get(i), ref velFilter.Pools.Inc2.Get(i));
            foreach (int i in moveFilter.Value)
                Accelerate(ref moveFilter.Pools.Inc1.Get(i), ref moveFilter.Pools.Inc2.Get(i));
        }

        void Move(ref ECPosition pos, ref ECVelocity vel)
        {
            if (!vel.IsMoving)
                return;
            pos.position += vel.velocity * Time.fixedDeltaTime;
        }
        void Accelerate(ref ECVelocity vel, ref ECAcceleration accel)
        {
            vel.velocity = Vector2.MoveTowards(vel.velocity, accel.direction * accel.targetSpeed, accel.acceleration*Time.fixedDeltaTime);
        }

    }
    public struct ECAcceleration
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
}