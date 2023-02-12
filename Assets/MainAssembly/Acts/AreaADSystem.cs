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
    //Action Delivery
    public class AreaADSystem : IEcsRunSystem
    {


        readonly EcsFilterInject<Inc<AMUsed, ACAreaDelivery>> useFilter = "act";
        readonly EcsFilterInject<Inc<ECPosition, ECCollider>> victimFilter = default;

        public void Run(IEcsSystems systems)
        {

            foreach (int i in useFilter.Value)
                foreach (var usage in useFilter.Pools.Inc1.Get(i).usages)
                    ProcessActUse(i, usage, useFilter.Pools.Inc2.Get(i));

        }

        void ProcessActUse(int act, ActUsageContainer usage, ACAreaDelivery area)
        {
            if (usage.targetType != ActTargetType.point)
                throw new Exception("Area AD system can only process point target type");
            int ent = ActService.GetEntity(act);
            var target = usage.vector;
            var pos = EcsStatic.GetPool<ECPosition>().Get(ent).position2;
            var dir = (target - pos).normalized;
            if (area.areaType == AreaType.concave)
                target = dir * area.radius / 2 + pos;
            foreach (int i in victimFilter.Value)
            {
                Team team = TeamService.GetTeam(i);
                if (!TeamService.FilterContains(area.teamFilter, team))
                    continue;
                var collider = victimFilter.Pools.Inc2.Get(i);
                var vicPos = victimFilter.Pools.Inc1.Get(i).position2;
                if (area.areaType == AreaType.circle)
                {
                    var dist = (vicPos - target).magnitude;
                    var colRad = collider.ApproximateRadius;
                    if (dist - colRad > area.radius || dist + colRad < area.exclusiveRadius)
                        continue;
                }
                else
                {
                    vicPos = Vector2.MoveTowards(vicPos, target, collider.ApproximateRadius);
                    var vicDir = vicPos - pos;
                    var dist = vicDir.magnitude;
                    if (dist > area.radius || dist < area.exclusiveRadius)
                        continue;
                    if (Vector2.Dot(vicDir, dir) / dist < area.concaveDot)
                        continue;
                }
                ActService.GetPool<AMEntityHit>().SafeAdd(act).victims.Add(EcsStatic.world.PackEntity(i));
            }
            int visualizer = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<ECDestroyDelayed>().Add(visualizer).time = 0.5f;
            EcsStatic.GetPool<ECPosition>().Add(visualizer).position2 = target;
            EcsStatic.GetPool<ECRenderMesh>().Add(visualizer).meshId = (int)MeshService.basicMesh.Sphere;

        }
    }
    public struct ACAreaDelivery
    {
        public Team teamFilter;
        public AreaType areaType;
        public float radius;
        public float concaveDot;
        public float concaveAngle { get => Mathf.Acos(concaveDot); set => concaveDot = Mathf.Cos(value); }
        public float exclusiveRadius;
    }
    public enum AreaType
    {
        circle,
        concave,
    }
}