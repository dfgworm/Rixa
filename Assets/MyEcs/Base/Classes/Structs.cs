using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

public struct EcsEntity
{
    public int entity;
    public EcsWorld world;
    public EcsEntity(int _ent, EcsWorld _w)
    {
        entity = _ent;
        world = _w;
    }
    public static bool Unpack(EcsPackedEntity packed, EcsWorld world, out EcsEntity entity)
    {
        entity = new EcsEntity();
        entity.world = world;
        if (!packed.Unpack(world, out int ent))
            return false;
        entity.entity = ent;
        return true;
    }
    public EcsPackedEntity Pack()
    {
        return world.PackEntity(entity);
    }
    public ref T Get<T>() where T : struct
    {
        return ref world.GetPool<T>().Get(entity);
    }
    public bool Has<T>() where T : struct
    {
        return world.GetPool<T>().Has(entity);
    }
    public ref T Add<T>() where T : struct
    {
        return ref world.GetPool<T>().Add(entity);
    }
    public ref T SafeAdd<T>() where T : struct
    {
        return ref world.GetPool<T>().SafeAdd(entity);
    }
    public void Del<T>() where T : struct
    {
        world.GetPool<T>().Del(entity);
    }
    public bool SafeDel<T>() where T : struct
    {
        return world.GetPool<T>().SafeDel(entity);
    }
}
public struct FloatLimited
{
    float _current;
    public float max;
    public FloatLimited(float amount)
    {
        _current = amount;
        max = amount;
    }
    public float Current
    {
        get => _current;
        set => _current = Mathf.Clamp(value, 0, max);
    }

    public float Percent
    {
        get => _current / max;
        set => _current = Mathf.Clamp01(value) * max;
    }
}
public struct IntLimited
{
    int _current;
    public int max;
    public IntLimited(int amount)
    {
        _current = amount;
        max = amount;
    }
    public int Current
    {
        get => _current;
        set => _current = Mathf.Clamp(value, 0, max);
    }

    public float Percent
    {
        get => _current / max;
        set => _current = Mathf.FloorToInt(Mathf.Clamp01(value) * max);
    }
}
public struct FloatRange
{
    public float min;
    public float max;
    public FloatRange(float _min, float _max)
    {
        min = _min;
        max = _max;
    }
    public float Clamp(float num)
        => Mathf.Clamp(num, min, max);
    public bool IsInRange(float num)
        => num >= min && num <= max;
}
