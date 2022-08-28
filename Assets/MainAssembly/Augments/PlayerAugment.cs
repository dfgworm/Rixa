﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using Mirror;
using MyEcs.Net;
using MyEcs.Spawn;

public class PlayerAugment : MonoBehaviour, ISpawnAugment, IDestroyAugment
{
    public float moveSpeed = 5f;
    public void SpawnAugment(int ent)
    {
        Debug.Log("Player Augmenting");

        var world = EcsStatic.world;
        ref var netOwner = ref EcsStatic.GetPool<ECNetOwner>().Get(ent);
        var pb = PlayerBehaviour.GetById(netOwner.playerId);

        world.GetPool<ECPlayerBehaviour>().Add(ent).pb = pb;
        pb.entity = world.PackEntity(ent);
        if (netOwner.BelongsToLocalPlayer)
            world.GetPool<ECLocalPlayer>().Add(ent);

        EcsStatic.GetPool<ECNetAutoSpawn>().Add(ent);
        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);
        if (netOwner.BelongsToLocalPlayer)
        {
            EcsStatic.GetPool<ECMover>().Add(ent).speed = moveSpeed;
            EcsStatic.GetPool<ECSyncSend>().Add(ent).payload = new BaggagePayload().Add(new PositionBaggage());
        }
        else if (NetStatic.IsServer)
        {
            EcsStatic.GetPool<ECSyncReceive>().Add(ent);
            ref var send = ref EcsStatic.GetPool<ECInterpolatePositionSend>().Add(ent);
            send.sendPeriod = 1 / 15f;
            send.exceptionConnectionId = pb.connection.connectionId;
        }
        else
        {
            EcsStatic.GetPool<ECInterpolatePositionReceive>().Add(ent).Reset(EcsStatic.GetPool<ECPosition>().Get(ent).position, (float)NetworkTime.time);
        }

    }
    public void DestroyAugment(int ent)
    {

    }
}
