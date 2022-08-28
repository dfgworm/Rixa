using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

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
        else
        {
            EcsStatic.GetPool<ECSyncReceive>().Add(ent);
            if (NetStatic.IsServer)
            {
                ref var send = ref EcsStatic.GetPool<ECSyncSend>().Add(ent);
                send.payload = new BaggagePayload().Add(new PositionBaggage());
                send.exceptConnectionId = pb.connection.connectionId;
            }
        }

    }
    public void DestroyAugment(int ent)
    {

    }
}
