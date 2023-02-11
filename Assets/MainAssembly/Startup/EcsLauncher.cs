using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using MyEcs.Physics;
using MyEcs.Health;
using MyEcs.Act;

public class EcsLauncher
{
    IEcsSystems updateSystems;
    IEcsSystems fixedUpdateSystems;

    public void Start()
    {
        Debug.Log("EcsLauncher Starting");
        EcsStatic.Load();
        ActService.Load();
        MeshService.Load();
        PrefabRegistry.Init();
        var bus = EcsStatic.bus;

        updateSystems = EcsStatic.updateSystems

            
            .Add(new PlayerInputSystem())
            .Add(new EntityMouseHoverSystem())
            .Add(new LocalControllableSystem())
            .Add(new LocalCameraSystem())



            .Add(new ActChannellingSystem())

            .Add(new InstantADSystem())
            .Add(new ProjectileADSystem())

            .Add(new DashActSystem())
            .Add(new HealEffectSystem())
            .Add(new DamageEffectSystem())
            .Add(new SpawnWallEffectSystem())
            .DelHere<AMUsed>("act")
            .DelHere<AMEntityHit>("act")
            .DelHere<AMPointHit>("act")
            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<InpActUse>()
            )

            .Add(new HealthSystem())
            .Add(new HealthDisplaySystem())
            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<EVDamage>()
                )


            .Add(new DestroySystem())

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
        ActService.Unload();
        Debug.Log("EcsLauncher Destroyed");
    }
}
