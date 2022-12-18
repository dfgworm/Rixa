using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using Mirror;
using MyEcs.Net;
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
        ref var netOwner = ref world.GetPool<ECNetOwner>().Get(ent);
        var pb = PlayerBehaviour.GetById(netOwner.playerId);

        world.GetPool<ECPlayerBehaviour>().Add(ent).pb = pb;
        pb.entity = world.PackEntity(ent);
        if (netOwner.BelongsToLocalPlayer)
            world.GetPool<ECLocalControllable>().Add(ent);

        world.GetPool<ECNetAutoSpawn>().Add(ent);

        PositionPipe.BuildPosition(world, ent, true);
        PositionPipe.BuildNetPosition(world, ent, new PositionPipe.NetPosArgs
        {
            syncPeriodFromServer = syncPeriodFromServer,
        });
        HealthPipe.BuildHealth(world, ent, max: health, regen: regen);
        HealthPipe.BuildNetHealth(world, ent);

        world.GetPool<ECTouchDamage>().Add(ent).dps = 10;

        ref var col = ref world.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(collisionSize, 0);

        var go = EcsGameObjectService.GetGameObject(ent);
        go.transform.localScale = new Vector3(col.size.x * 2, 1, col.size.x * 2);

        if (netOwner.BelongsToLocalPlayer)
        {
            world.GetPool<ECSyncSend>().Get(ent).sendPeriod = syncPeriodFromClient;

            ref var acc = ref world.GetPool<ECTargetVelocity>().Add(ent);
            acc.targetSpeed = moveSpeed;
            acc.acceleration = 25;
            world.GetPool<ECVelocity>().Add(ent);
            world.GetPool<ECRespectObstacles>().Add(ent);
        }
        else
        {

        }

        ref var channelDisplay = ref EcsStatic.GetPool<ECChannelDisplay>().Add(ent);
        channelDisplay.Init();
        channelDisplay.controller.shift = new Vector3(0, 3, 0);

        GiveAbility(world, ent);
    }
    void GiveAbility(EcsWorld world, int ent)
    {
        ref var netOwner = ref world.GetPool<ECNetOwner>().Get(ent);

        var acWorld = EcsStatic.GetWorld("actions");
        int acEnt = EcsActionService.CreateAction(ent);
        //if (netOwner.BelongsToLocalPlayer)
        //    acWorld.GetPool<ACLocalControllable>().Add(acEnt).targetType = ActionTargetType.direction;
        //ref var proj = ref acWorld.GetPool<ACProjectileDelivery>().Add(acEnt);
        //proj.lifetime = 5;
        //proj.selfDestruct = true;
        //proj.velocity = 5;
        //ref var heal = ref acWorld.GetPool<ACDamage>().Add(acEnt);
        //heal.amount = 20;
        ref var dash = ref acWorld.GetPool<ACDash>().Add(acEnt);
        dash.range = 8;
        dash.velocity = 16;


        int channelAc = EcsActionService.CreateAction(ent);
        EcsActionService.GetPool<ACLocalControllable>().Add(channelAc).targetType = ActionTargetType.point;
        ref var channel = ref EcsActionService.GetPool<ACChannelled>().Add(channelAc);
        channel.duration = 0.2f;
        channel.finishAction = EcsActionService.acWorld.PackEntity(acEnt);
    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
