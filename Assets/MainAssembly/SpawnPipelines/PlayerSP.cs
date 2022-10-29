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

        PositionPipe.BuildPosition(world, ent, new PositionPipe.PosArgs {
            positionToTransform = true,
        });
        PositionPipe.BuildNetPosition(world, ent, new PositionPipe.NetPosArgs
        {
            syncPeriodFromServer = syncPeriodFromServer,
        });

        HealthPipe.BuildHealth(world, ent, new HealthPipe.HealthArgs
        {
            max = health,
            percent = 1,
            regen = regen,
        });
        HealthPipe.BuildNetHealth(world, ent);

        world.GetPool<ECTouchDamage>().Add(ent).dps = 10;

        ref var col = ref world.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(collisionSize, 0);
        if (netOwner.BelongsToLocalPlayer)
        {
            world.GetPool<ECSyncSend>().Get(ent).sendPeriod = syncPeriodFromClient;

            ref var acc = ref world.GetPool<ECAcceleration>().Add(ent);
            acc.targetSpeed = moveSpeed;
            acc.acceleration = 25;
            world.GetPool<ECVelocity>().Add(ent);
            world.GetPool<ECRespectObstacles>().Add(ent);
        }
        else
        {
            
        }

    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
