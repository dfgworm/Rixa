using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Spawn;
using MyEcs.Physics;
using MyEcs.Health;
using MyEcs.Actions;

public class PlayerSP : ScriptableObject, ISpawnPipeline
{
    public float moveSpeed = 5f;
    public float collisionSize = 1f;
    public float health = 100;
    public float regen = 1;

    public float syncPeriodFromClient = 1 / 20f;
    public float syncPeriodFromServer = 1 / 20f;

    public void Spawn(EcsWorld world, int ent)
    {
        PositionPipe.BuildPosition(world, ent, true);
        HealthPipe.BuildHealth(world, ent, max: health, regen: regen);

        world.GetPool<ECLocalControllable>().Add(ent);
        world.GetPool<ECTouchDamage>().Add(ent).dps = 10;

        ref var col = ref world.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(collisionSize, 0);

        var go = EcsGameObjectService.GetGameObject(ent);
        go.transform.localScale = new Vector3(col.size.x * 2, 1, col.size.x * 2);


        ref var acc = ref world.GetPool<ECTargetVelocity>().Add(ent);
        acc.targetSpeed = moveSpeed;
        acc.acceleration = 25;
        world.GetPool<ECVelocity>().Add(ent);
        world.GetPool<ECRespectObstacles>().Add(ent);

        ref var channelDisplay = ref EcsStatic.GetPool<ECChannelDisplay>().Add(ent);
        channelDisplay.Init();
        channelDisplay.controller.shift = new Vector3(0, 3, 0);

        GiveDash(world, ent);
        GiveChargedShot(world, ent);
    }
    void GiveDash(EcsWorld world, int ent)
    {

        int acEnt = EcsActionService.CreateAction(ent);
        ref var dash = ref EcsActionService.GetPool<ACDash>().Add(acEnt);
        dash.range = 8;
        dash.velocity = 16;
        EcsActionService.GetPool<ACInputType>().Add(acEnt).targetType = ActionTargetType.point;
        PlayerInputSystem.ConnectActionToInput(PlayerInputSystem.controls.Player.Dash, acEnt);
    }
    void GiveChargedShot(EcsWorld world, int ent)
    {

        int acEnt = EcsActionService.CreateAction(ent);
        ref var proj = ref EcsActionService.GetPool<ACProjectileDelivery>().Add(acEnt);
        proj.lifetime = 5;
        proj.selfDestruct = true;
        proj.velocity = 5;
        ref var dmg = ref EcsActionService.GetPool<ACDamage>().Add(acEnt);
        dmg.amount = 20;

        int channelAc = EcsActionService.CreateAction(ent);
        EcsActionService.GetPool<ACInputType>().Add(channelAc).targetType = ActionTargetType.direction;
        PlayerInputSystem.ConnectActionToInput(PlayerInputSystem.controls.Player.SecondaryAttack, channelAc);
        ref var channel = ref EcsActionService.GetPool<ACChannelled>().Add(channelAc);
        channel.duration = 0.5f;
        channel.finishAction = EcsActionService.acWorld.PackEntity(acEnt);
    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
