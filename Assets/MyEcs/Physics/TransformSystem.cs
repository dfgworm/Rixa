using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace MyEcs.Physics
{
    public class TransformSystem : IEcsRunSystem
    {
        EcsFilterInject<Inc<ECGameObject, ECPosition, ECPositionToTransform>> posToTrFilter = default;
        EcsFilterInject<Inc<ECGameObject, ECPosition, ECTransformToPosition>> trToPosFilter = default;

        EcsFilterInject<Inc<ECGameObject, ECRotation, ECRotationToTransform>> rotToTrFilter = default;
        EcsFilterInject<Inc<ECGameObject, ECRotation, ECTransformToRotation>> trToRotFilter = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in posToTrFilter.Value)
                ApplyPosition(ref posToTrFilter.Pools.Inc1.Get(i), ref posToTrFilter.Pools.Inc2.Get(i), ref posToTrFilter.Pools.Inc3.Get(i));
            foreach (int i in rotToTrFilter.Value)
                ApplyRotation(ref rotToTrFilter.Pools.Inc1.Get(i), ref rotToTrFilter.Pools.Inc2.Get(i), ref rotToTrFilter.Pools.Inc3.Get(i));
            foreach (int i in trToPosFilter.Value)
                ReflectPosition(ref trToPosFilter.Pools.Inc1.Get(i), ref trToPosFilter.Pools.Inc2.Get(i), ref trToPosFilter.Pools.Inc3.Get(i));
            foreach (int i in trToRotFilter.Value)
                ReflectRotation(ref trToRotFilter.Pools.Inc1.Get(i), ref trToRotFilter.Pools.Inc2.Get(i), ref trToRotFilter.Pools.Inc3.Get(i));

        }

        void ApplyPosition(ref ECGameObject link, ref ECPosition pos, ref ECPositionToTransform appl)
        {
            link.gameObject.transform.position = new Vector3(pos.position.x, 0, pos.position.y) + appl.shift;
        }
        void ApplyRotation(ref ECGameObject link, ref ECRotation rot, ref ECRotationToTransform appl)
        {
            //link.gameObject.transform.rotation = rot.rotation;
        }
        void ReflectPosition(ref ECGameObject link, ref ECPosition pos, ref ECTransformToPosition refl)
        {
            var p = link.gameObject.transform.position;
            pos.position = new Vector2(p.x, p.z) + refl.shift;
        }
        void ReflectRotation(ref ECGameObject link, ref ECRotation rot, ref ECTransformToRotation refl)
        {
            //rot.rotation = link.gameObject.transform.rotation;
        }
    }
    public struct ECPositionToTransform
    {
        public Vector3 shift;
    }
    public struct ECRotationToTransform
    {

    }
    public struct ECTransformToPosition
    {
        public Vector2 shift;
    }
    public struct ECTransformToRotation
    {

    }
}