using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Net;
using MyEcs.Spawn;
using MyEcs.Physics;
using MyEcs.Health;

public class EnemySP : ScriptableObject, ISpawnPipeline
{
    public float health = 100;
    public void Spawn(EcsWorld world, int ent)
    {
        world.GetPool<ECNetAutoSpawn>().Add(ent);


        PositionPipe.BuildPosition(world, ent, true);

        var model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        EcsGameObjectService.Link(world, ent, model);

        ref var col = ref world.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(1, 0);
        model.transform.localScale = new Vector3(col.size.x*2, 1, col.size.x*2);

        

        world.GetPool<ECTouchDamage>().Add(ent).dps = 2;

        HealthPipe.BuildHealth(world, ent, new HealthPipe.HealthArgs
        {
            max = health,
        });
        HealthPipe.BuildNetHealth(world, ent);
    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
