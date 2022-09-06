using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

public static class EcsGameObjectService
{
    public static void Link(int entity, GameObject go) => Link(EcsStatic.world, entity, go);
    public static void Link(EcsWorld world, int entity, GameObject go)
    {
        SetEntity(go, world, entity);
        SetGameObject(world, entity, go);
    }
    public static void SetEntity(GameObject gb, int entity) => SetEntity(gb, EcsStatic.world, entity);
    public static void SetEntity(GameObject gb, EcsWorld world, int entity)
    {
        EcsPointer pointer = gb.GetComponent<EcsPointer>();
        if (pointer == null)
            pointer = gb.AddComponent<EcsPointer>();
        pointer.entity = world.PackEntity(entity);
        pointer.world = world;
    }

    public static int GetEntity(GameObject gobj)
    {
        if (TryGetEntity(gobj, out int ent))
            return ent;
        else
            return -1;
    }
    public static bool TryGetEntity(GameObject gobj, out int ent)
    {
        ent = -1;
        if (gobj == null) return false;
        EcsPointer pointer = gobj.GetComponent<EcsPointer>();
        if (pointer == null)
            return false;
        if (pointer.entity.Unpack(pointer.world, out ent))
            return true;
        else
            return false;
    }
    public static void SetGameObject(int entity, GameObject go) => SetGameObject(EcsStatic.world, entity, go);
    public static void SetGameObject(EcsWorld world, int entity, GameObject go)
    {
        var pool = world.GetPool<ECGameObject>();
        if (pool.Has(entity))
            pool.Get(entity).gameObject = go;
        else
            pool.Add(entity).gameObject = go;
    }
    public static GameObject GetGameObject(int entity) => GetGameObject(EcsStatic.world, entity);
    public static GameObject GetGameObject(EcsWorld world, int entity)
    {
        var pool = world.GetPool<ECGameObject>();
        if (!pool.Has(entity))
            return null;
        return pool.Get(entity).gameObject;
    }
}
