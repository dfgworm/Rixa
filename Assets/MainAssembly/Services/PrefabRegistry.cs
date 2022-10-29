using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Spawn
{
    public enum PrefabId : byte
    {
        unknown,
        player,

        _count,
    }
    public static class PrefabRegistry
    {
        static GameObject[] prefabs;
        static bool _loaded = false;
        public static void Init()
        {
            if (_loaded)
                return;
            prefabs = new GameObject[(int)PrefabId._count];

            Register(PrefabId.player, "Player_Model");

            _loaded = true;
        }
        public static GameObject GetPrefab(PrefabId id)
        {
            return prefabs[(int)id];
        }
        public static void Clear()
        {
            if (!_loaded)
                return;
            _loaded = false;
            foreach (GameObject go in prefabs)
                Resources.UnloadAsset(go);
            prefabs = null;
        }
        public static void Register(PrefabId id, string path)
        {
            prefabs[(int)id] = Resources.Load<GameObject>("Prefabs/" + path);
        }
    }
    public class PrefabIdBaggage : IBaggageAutoUnload
    {
        public PrefabId id;

        public void UnloadToWorld(EcsWorld world, int ent)
        {
            var go = GameObject.Instantiate(PrefabRegistry.GetPrefab(id));
            EcsGameObjectService.SetGameObject(ent, go);
            EcsGameObjectService.SetEntity(go, ent);
        }
    }
}