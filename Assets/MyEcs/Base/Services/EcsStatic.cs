using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leopotam.EcsLite;
public static class EcsStatic
{
    public static EcsWorld world;
    public static IEcsSystems updateSystems;
    public static IEcsSystems fixedUpdateSystems;
    public static EventBus bus;

    static bool _loaded;
    public static void Load()
    {
        if (_loaded)
            throw new Exception(typeof(EcsStatic).Name+" already loaded");
        _loaded = true;
        bus = new EventBus();
        world = new EcsWorld();
        updateSystems = new EcsSystems(world);
        fixedUpdateSystems = new EcsSystems(world);
    }
    public static void Unload()
    {
        if (!_loaded)
            throw new Exception(typeof(EcsStatic).Name + " was not loaded");
        _loaded = false;
        updateSystems.Destroy();
        fixedUpdateSystems.Destroy();
        world.Destroy();
        bus.Destroy();
    }
    public static EcsEntity NewEntity()
        => new EcsEntity(world.NewEntity(), world);
    public static EcsEntity GetEntity(int _ent)
        => new EcsEntity(_ent, world);
    public static bool Unpack(EcsPackedEntity packed, out EcsEntity entity)
        => EcsEntity.Unpack(packed, world, out entity);
    public static bool UnpackEntity(this EcsPackedEntity packed, out EcsEntity ent)
        => Unpack(packed, out ent);
    public static EcsWorld AddWorld(string name)
    {
        var world = new EcsWorld();
        updateSystems.AddWorld(world, name);
        fixedUpdateSystems.AddWorld(world, name);
        return world;
    }
    public static EcsWorld GetWorld(string name)
        => updateSystems.GetWorld(name);
    public static EcsPool<T> GetPool<T>() where T:struct
    {
        return world.GetPool<T>();
    }
    static object[] args = new object[0];
    public static void LoadPool(string worldName, Type type)
    {
        var world = updateSystems.GetWorld(worldName);
        world.GetType().GetMethod("GetPool").MakeGenericMethod(type).Invoke(world, args);
    }
}

