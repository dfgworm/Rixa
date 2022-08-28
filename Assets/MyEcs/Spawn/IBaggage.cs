using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Spawn
{
    public interface IBaggage : Mirror.NetworkMessage
    {
        public void UnloadToWorld(EcsWorld world, int ent);
    }
    public class BaggagePayload
    {
        public BaggagePayload()
        {
            List = new List<IBaggage>(4);
        }
        public List<IBaggage> List { get; set; }
        public BaggagePayload Add<T>(T el) where T : IBaggage
        {
#if DEBUG
            if (Has<T>())
                throw new Exception(typeof(T).Name+" type dublicate adding to payload");
#endif
            List.Add(el);
            return this;
        }
        public bool Has<T>() where T : IBaggage
        {
            return List.Exists(a => a is T);
        }
        public T Get<T>() where T : IBaggage
        {
            return (T)List.Find(a => a is T);
        }
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            foreach (var bag in List)
                bag.UnloadToWorld(world, ent);
        }
    }
}