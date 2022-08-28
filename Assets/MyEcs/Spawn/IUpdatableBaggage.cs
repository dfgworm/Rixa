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
}