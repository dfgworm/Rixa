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
using MyEcs.Acts;

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



            .Add(new ChannellingSystem())

            .Add(new InstantADSystem())
            .Add(new ProjectileADSystem())

            .Add(new DashActSystem())
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


            .Add(new DestroySystem())
            .Add(bus.GetDestroyEventsSystem()
                .IncReplicant<EVDamage>()
                .IncReplicant<InpActUse>()
                )

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
