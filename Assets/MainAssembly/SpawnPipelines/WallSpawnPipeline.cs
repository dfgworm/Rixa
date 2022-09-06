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

public class WallSpawnPipeline : ScriptableObject, ISpawnPipeline
{
    public void Spawn(EcsWorld world, int ent)
    {
        EcsStatic.GetPool<ECNetAutoSpawn>().Add(ent);

        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);
        EcsStatic.GetPool<ECObstacle>().Add(ent);
        var model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        EcsGameObjectService.Link(world, ent, model);
        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.rectangle;
        col.size = new Vector2(1, 1);
        model.transform.localScale = new Vector3(col.size.x, 1, col.size.y);


    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
