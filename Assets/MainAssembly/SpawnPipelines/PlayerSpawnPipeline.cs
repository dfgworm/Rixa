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

public class PlayerSpawnPipeline : ScriptableObject, ISpawnPipeline
{
    public float moveSpeed = 5f;
    public float collisionSize = 1f;
    public float syncPeriodFromClient = 1 / 20f;
    public float syncPeriodFromServer = 1 / 20f;

    public void Spawn(EcsWorld world, int ent)
    {

        ref var netOwner = ref EcsStatic.GetPool<ECNetOwner>().Get(ent);
        var pb = PlayerBehaviour.GetById(netOwner.playerId);

        world.GetPool<ECPlayerBehaviour>().Add(ent).pb = pb;
        pb.entity = world.PackEntity(ent);
        if (netOwner.BelongsToLocalPlayer)
            world.GetPool<ECLocalControllable>().Add(ent);

        EcsStatic.GetPool<ECNetAutoSpawn>().Add(ent);
        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);
        if (netOwner.BelongsToLocalPlayer)
        {
            ref var acc = ref EcsStatic.GetPool<ECAcceleration>().Add(ent);
            acc.targetSpeed = moveSpeed;
            acc.acceleration = 25;
            EcsStatic.GetPool<ECVelocity>().Add(ent);
            EcsStatic.GetPool<ECRespectObstacles>().Add(ent);
            ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
            col.type = ColliderType.circle;
            col.size = new Vector2(collisionSize, 0);

            ref var send = ref EcsStatic.GetPool<ECSyncSend>().Add(ent);
            send.sendPeriod = syncPeriodFromClient;
            send.payload = new BaggagePayload().Add(new PositionBaggage());

            //LoadCameraFocus(world); moved it to InitialSpawn. Tying it to player seems meaningsless, should find another place
        }
        else if (NetStatic.IsServer)
        {
            EcsStatic.GetPool<ECSyncReceive>().Add(ent);
            ref var send = ref EcsStatic.GetPool<ECInterpolatePositionSend>().Add(ent);
            send.sendPeriod = syncPeriodFromServer;
            send.exceptionConnectionId = pb.connection.connectionId;
        }
        else
        {
            EcsStatic.GetPool<ECInterpolatePositionReceive>().Add(ent).Reset(EcsStatic.GetPool<ECPosition>().Get(ent).position, (float)NetworkTime.time);
        }

    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
