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
        public void LoadToBaggage(EcsWorld world, int ent);
        public bool IsUpToDate(EcsWorld world, int ent);
    }
    public static class BaggagePayloadUpdatableExtension
    {
        public static T EnsureBaggage<T>(this BaggagePayload payload, EcsWorld world, int ent)
            where T : IUpdatableBaggage, new()
        {
            T baggage = payload.Get<T>();
            if (baggage != null)
                baggage.UnloadToWorld(world, ent);
            else
            {
                baggage = new T();
                baggage.LoadToBaggage(world, ent);
                payload.Add(baggage);
            }
            return baggage;
        }
        public static void UpdateThis(this BaggagePayload payload, EcsWorld world, int ent)
        {
            foreach (var bag in payload.List.OfType<IUpdatableBaggage>())
                bag.LoadToBaggage(world, ent);
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