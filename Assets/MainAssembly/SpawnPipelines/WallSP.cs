using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Spawn;
using MyEcs.Physics;

public class WallSP : ScriptableObject, ISpawnPipeline
{
    public void Spawn(EcsWorld world, int ent)
    {

        PositionPipe.BuildPosition(world, ent);
        EcsStatic.GetPool<ECObstacle>().Add(ent);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.rectangle;
        col.size = new Vector2(1, 1);

        ref var mesh = ref EcsStatic.GetPool<ECRenderMesh>().Add(ent);
        mesh.meshId = 0;
        mesh.rotation = Quaternion.identity;
        mesh.scale = Vector3.one;

    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
