using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Spawn;
using MyEcs.Physics;
using MyEcs.Health;

public class EnemySP : ScriptableObject, ISpawnPipeline
{
    public float health = 100;
    public void Spawn(EcsWorld world, int ent)
    {


        PositionPipe.BuildPosition(world, ent, true);

        var model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        EcsGameObjectService.Link(world, ent, model);

        ref var col = ref world.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(1, 0);
        model.transform.localScale = new Vector3(col.size.x*2, 1, col.size.x*2);

        ref var hover = ref world.GetPool<ECMouseHoverable>().Add(ent);
        hover.radius = col.size.x;
        

        world.GetPool<ECTouchDamage>().Add(ent).dps = 2;

        HealthPipe.BuildHealth(world, ent, max: health);
    }
    public void Destroy(EcsWorld world, int ent)
    {

    }
}
