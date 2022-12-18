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
using MyEcs.Physics;
using MyEcs.Net;
using MyEcs.Health;
using MyEcs.Actions;

public class EcsLauncher
{
    IEcsSystems updateSystems;
    IEcsSystems fixedUpdateSystems;

    public void Start()
    {
        Debug.Log("EcsLauncher Starting");
        EcsStatic.Load();
        PrefabRegistry.Init();
        EcsActionService.Load();
        MeshService.Load();
        var bus = EcsStatic.bus;

        updateSystems = EcsStatic.updateSystems

            .Add(new SyncReceiveSystem())
            
            .Add(new PlayerInputSystem())
            .Add(new EntityHoverSystem())
            .Add(new LocalControllableSystem())
            .Add(new LocalCameraSystem())



            .Add(new ChannellingSystem())
            .Add(new NetActionSystem())

            .Add(new InstantADSystem())
            .Add(new ProjectileADSystem())

            .Add(new DashActionSystem())
            .Add(new HealEffectSystem())
            .Add(new DamageEffectSystem())
            .Add(new SpawnWallEffectSystem())
            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<AEVUse>()
                .IncReplicant<AEVEntityHit>()
                .IncReplicant<AEVPointHit>()
            )

            .Add(new HealthSystem())
            .Add(new HealthDisplaySystem())

            .Add(new SpawnSystem())
            .Add(new SpawnPipelineSystem())
            .Add(new NetSpawnSystem())

            .Add(new NetDestroySystem())
            .Add(new DestroySystem())
            .DelHere<EMSpawned>()
            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<EVSpawn>()
                .IncReplicant<EVDamage>()
                .IncSingleton<InpActionUse>() //this should be a replicant as well
                )

            .Add(new InterpolatedPositionSystem())
            .Add(new SyncSendSystem())

            .Add(new RenderSystem())
            ;

        fixedUpdateSystems = EcsStatic.fixedUpdateSystems
            .Add(new MovementSystem())
            .Add(new RotationSystem())
            .Add(new TransformSystem())

            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<EVCollision>()
                )
            .Add(new CollisionSystem())
            .Add(new HealthPhysicsSystem())
            .Add(new MovementObstacleSystem())

            ;
        EcsStatic.updateSystems = updateSystems;
        EcsStatic.fixedUpdateSystems = fixedUpdateSystems;

        updateSystems.Inject(bus)
            .Init();
        fixedUpdateSystems.Inject(bus)
            .Init();

        InitialSpawn.Spawn();
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
        MeshService.Unload();
        EcsStatic.Unload();
        EcsActionService.Unload();
        Debug.Log("EcsLauncher Destroyed");
    }
}
