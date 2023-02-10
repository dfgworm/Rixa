using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Physics;


public static class WallSP
{
    public static int Spawn(Vector2 pos = new Vector2())
    {
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(ent).position2 = pos;
        EcsStatic.GetPool<ECObstacle>().Add(ent);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.rectangle;
        col.size = new Vector2(1, 1);

        ref var mesh = ref EcsStatic.GetPool<ECRenderMesh>().Add(ent);
        mesh.meshId = 0;
        mesh.rotation = Quaternion.identity;
        mesh.scale = Vector3.one;
        return ent;
    }
}
