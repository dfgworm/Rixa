using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using MyEcs.Spawn;
using MyEcs.Net;

public class EcsLauncher
{
    EcsWorld world;
    IEcsSystems updateSystems;
    IEcsSystems fixedUpdateSystems;
    EventBus bus;

    public void Start()
    {
        Debug.Log("EcsSystemService Starting");
        EcsStatic.Load();
        PrefabRegistry.Init();
        world = EcsStatic.world;
        bus = EcsStatic.bus;

        updateSystems = EcsStatic.updateSystems

            .Add(new PlayerInputSystem())

            .Add(new SpawnSystem())
            .Add(new SpawnPipelineSystem())
            .Add(new NetSpawnSystem())
            //.Add(new DebugSystem())

            .Add(new NetDestroySystem())
            .Add(new DestroySystem())
            .DelHere<EMSpawned>()
            .Add(bus.GetDestroyEventsSystem().IncReplicant<EVSpawn>())
            //.Add(bus.GetClearSystem())

            .Add(new InterpolatedPositionSystem())
            .Add(new SyncSendSystem())
            .Add(new SyncReceiveSystem())
            ;
        fixedUpdateSystems = EcsStatic.fixedUpdateSystems
            .Add(new PositionSystem())
            .Add(new RotationSystem())
            .Add(new TransformSystem())
            ;
        EcsStatic.updateSystems = updateSystems;
        EcsStatic.fixedUpdateSystems = fixedUpdateSystems;

        updateSystems.Inject(bus)
            .Init();
        fixedUpdateSystems.Inject(bus)
            .Init();

    }
    public void Update()
    {
        updateSystems.Run();
    }

    public void FixedUpdate()
    {
        fixedUpdateSystems.Run();
    }

    public void Destroy()
    {
        EcsStatic.Unload();
        Debug.Log("EcsSystemService Destroyed");
    }
}
