using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Linq;

namespace MyEcs.Spawn
{
    public interface IUpdatableBaggage :IBaggage
    {
        public void UpdateThis(EcsWorld world, int ent);
        public bool IsUpToDate(EcsWorld world, int ent);
    }
    public static class BaggagePayloadUpdatableExtension
    {
        public static void UpdateThis(this BaggagePayload payload, EcsWorld world, int ent)
        {
            foreach (var bag in payload.List.OfType<IUpdatableBaggage>())
                bag.UpdateThis(world, ent);
        }
        public static int GetNeedsUpdate(this BaggagePayload payload, EcsWorld world, int ent, List<IBaggage> outload)
        {
            foreach (var bag in payload.List.OfType<IUpdatableBaggage>())
                if (!bag.IsUpToDate(world, ent))
                    outload.Add(bag);
            return outload.Count;
        }
    }
    public class PositionBaggage : IUpdatableBaggage
    {
        public Vector2 position;
        public void UpdateThis(EcsWorld world, int ent)
        {
            var pool = world.GetPool<ECPosition>();
            position = pool.Get(ent).position;
        }
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECPosition>().SoftAdd(ent).position = position;
        }
        public bool IsUpToDate(EcsWorld world, int ent)
        {
            return world.GetPool<ECPosition>().Get(ent).position.FuzzyEquals(position);
        }
    }
    public class RotationBaggage : IUpdatableBaggage
    {
        public float rotation;
        public void UpdateThis(EcsWorld world, int ent)
        {
            var pool = world.GetPool<ECRotation>();
            rotation = pool.Get(ent).rotation;
        }
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECRotation>().SoftAdd(ent).rotation = rotation;
        }
        public bool IsUpToDate(EcsWorld world, int ent)
        {
            return world.GetPool<ECRotation>().Get(ent).rotation == rotation;
        }
    }
}