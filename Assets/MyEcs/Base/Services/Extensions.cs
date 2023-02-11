using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

using Leopotam.EcsLite;

public static class VectorExtentions
{
    public static bool FuzzyEquals(this Vector2 v1, Vector2 v2)
    {
        return Mathf.Abs(v1.x - v2.x) < Vector2.kEpsilon && Mathf.Abs(v1.y - v2.y) < Vector2.kEpsilon;
    }
    public static Vector3 Vec3(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
    public static bool FuzzyEquals(this Vector3 v1, Vector3 v2)
    {
        return Mathf.Abs(v1.x - v2.x) < Vector3.kEpsilon && Mathf.Abs(v1.y - v2.y) < Vector3.kEpsilon && Mathf.Abs(v1.z - v2.z) < Vector3.kEpsilon;
    }
    public static Vector2 Vec2(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

}


public static class PoolExtensions
{
    public static ref T SafeAdd<T>(this EcsPool<T> pool, int ent) where T : struct
    {
        if (pool.Has(ent))
            return ref pool.Get(ent);
        else
            return ref pool.Add(ent);
    }
    public static bool SafeDel<T>(this EcsPool<T> pool, int ent) where T : struct
    {
        bool has = pool.Has(ent);
        if (has)
            pool.Del(ent);
        return has;
    }

}


public static class FilterExtensions
{
    public struct EntityPair
    {
        public int i;
        public int j;
    }
    public static PairEnumerator EnumPairsWithSelf(this EcsFilter filter) //does not support editing affected pools
    {
        return new PairEnumerator(filter);
    }
    public struct PairEnumerator
    {
        readonly int[] _entities;
        readonly int _count;
        int _idx1;
        int _idx2;

        public PairEnumerator GetEnumerator()
        {
            return this;
        }

        public PairEnumerator(EcsFilter filter)
        {
            _entities = filter.GetRawEntities();
            _count = filter.GetEntitiesCount();
            _idx1 = 0;
            _idx2 = 0;
        }

        public EntityPair Current
        {
            get => new EntityPair { i = _entities[_idx1], j = _entities[_idx2] };
        }

        public bool MoveNext()
        {
            _idx1++;
            if (_idx1 >= _count)
            {
                _idx2++;
                _idx1 = _idx2+1;
                return _idx1 < _count;
            }
            return true;
        }

    }
}
