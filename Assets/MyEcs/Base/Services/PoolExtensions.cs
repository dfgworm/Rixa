using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

using Leopotam.EcsLite;

public static class PoolExtensions
{
    public static ref T SoftAdd<T>(this EcsPool<T> pool, int ent) where T : struct
    {
        if (pool.Has(ent))
            return ref pool.Get(ent);
        else
            return ref pool.Add(ent);
    }
    public static bool SoftDel<T>(this EcsPool<T> pool, int ent) where T : struct
    {
        bool has = pool.Has(ent);
        if (has)
            pool.Del(ent);
        return has;
    }

}
