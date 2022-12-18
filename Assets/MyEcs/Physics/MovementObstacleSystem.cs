using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace MyEcs.Physics
{
    public class MovementObstacleSystem : IEcsRunSystem
    {
        public readonly static float defaultPushForce = 20;
        public readonly static float defaultStoppingFactor = 20;

        readonly EcsWorldInject world = default;

        readonly EcsPoolInject<ECPosition> posPool = default;
        readonly EcsPoolInject<ECVelocity> velPool = default;
        readonly EcsPoolInject<ECRespectObstacles> pushablePool = default;
        readonly EcsPoolInject<ECObstacle> obsPool = default;
        readonly EcsPoolInject<ECCollider> colPool = default;

        readonly EcsCustomInject<EventBus> bus = default;

        readonly EcsFilterInject<Inc<ECRespectObstacles>> pushableFilter = default;
        readonly EcsFilterInject<Inc<ECRespectObstacles, ECVelocity>> velocityFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (int i in pushableFilter.Value)
                ResetPushForce(ref pushableFilter.Pools.Inc1.Get(i));
            foreach (int i in bus.Value.GetEventBodies<EVCollision>(out var pool))
                ProcessCollision(ref pool.Get(i));
            foreach(int i in velocityFilter.Value)
                ApplyPushForce(ref velocityFilter.Pools.Inc1.Get(i), ref velocityFilter.Pools.Inc2.Get(i));


        }
        void ResetPushForce(ref ECRespectObstacles pushable)
        {
            pushable.force = Vector2.zero;
        }
        void ProcessCollision(ref EVCollision ev)
        {
            if (!ev.ent1.Unpack(world.Value, out int ent1) || !ev.ent2.Unpack(world.Value, out int ent2))
                return;
            if (IsMover(ent1) && IsObstacle(ent2))
                PushMover(ent1, ent2);
            if (IsMover(ent2) && IsObstacle(ent1))
                PushMover(ent2, ent1);
        }
        void ApplyPushForce(ref ECRespectObstacles pushable, ref ECVelocity vel)
        {
            //perhaps i need some kind of limitation here so it doesn't push the object too hard in case of high dt?
            //force = Mathf.min(force, vel.Dot(force.normalized));
            //but then why don't i just nullify the velocity along push direction?
            //Because i want the system to be soft so there are no stuck locks anywhere?
            //Should probably act only if problems are discovered
            vel.velocity += pushable.force * Time.fixedDeltaTime; 
        }
        bool IsMover(int ent)
        {
            return pushablePool.Value.Has(ent) && posPool.Value.Has(ent);
        }
        bool IsObstacle(int ent)
        {
            return obsPool.Value.Has(ent) && colPool.Value.Has(ent) && posPool.Value.Has(ent);
        }
        void PushMover(int ent, int obs)
        {
            ref var pos = ref posPool.Value.Get(ent);
            ref var pushable = ref pushablePool.Value.Get(ent);

            ref var obsPos = ref posPool.Value.Get(obs);
            ref var obsPush = ref obsPool.Value.Get(obs);
            ref var obsCol = ref colPool.Value.Get(obs);

            var pushVector = GetPushVector(ref obsPos, ref obsCol, ref pos);
            pushable.force += pushVector * obsPush.pushForce;

            if (velPool.Value.Has(ent))
            {
                var dot = Vector2.Dot(velPool.Value.Get(ent).velocity, pushVector);
                if (dot < 0)
                    pushable.force -= pushVector * dot * obsPush.stoppingFactor;
            }
        }

        public static Vector2 GetPushVector(ref ECPosition obsPos, ref ECCollider obsCol, ref ECPosition entPos)
        {
            if (obsCol.type == ColliderType.rectangle)
                return GetPushVectorRect(obsPos.position2, obsCol.size, entPos.position2);
            else
                return GetPushVector(obsPos.position2, entPos.position2);
        }
        public static Vector2 GetPushVectorRect(Vector2 obsPos, Vector2 obsSize, Vector2 entPos)
        {
            Vector2 halfSize = obsSize / 2;
            Vector2 diff = entPos - obsPos;
            Vector2 absDiff = new Vector2(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
            if (absDiff.x < halfSize.x && (absDiff.y >= halfSize.y || Mathf.Abs(absDiff.x - halfSize.x) < Mathf.Abs(absDiff.y - halfSize.y)))
                return new Vector2(0, Mathf.Sign(diff.y));
            if (absDiff.y < halfSize.y)
                return new Vector2(Mathf.Sign(diff.x), 0);
            Vector2 corner = obsPos + new Vector2(Mathf.Sign(diff.x), Mathf.Sign(diff.y)) * halfSize;
            return GetPushVector(corner, entPos);
        }
        public static Vector2 GetPushVector(Vector2 obsPos, Vector2 entPos)
        {
            var diff = entPos - obsPos;
            return diff.normalized;
        }
    }
    public struct ECRespectObstacles
    {
        public Vector2 force;
    }
    public struct ECObstacle : IEcsAutoReset<ECObstacle>
    {
        public float pushForce;
        public float stoppingFactor;

        public void AutoReset(ref ECObstacle c) {
            c.pushForce = MovementObstacleSystem.defaultPushForce;
            c.stoppingFactor = MovementObstacleSystem.defaultStoppingFactor;
        }
    }

}