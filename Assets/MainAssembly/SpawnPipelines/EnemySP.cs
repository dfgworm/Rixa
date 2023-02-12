using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Physics;
using MyEcs.Health;

public static class EnemySP
{
    public static float health = 100;
    public static int Spawn()
    {
        int ent = EcsStatic.world.NewEntity();
        TeamService.SetTeam(ent, Team.enemy);

        EcsStatic.GetPool<ECPosition>().Add(ent);
        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);

        var model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        EcsGameObjectService.Link(ent, model);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(1, 0);
        model.transform.localScale = new Vector3(col.size.x*2, 1, col.size.x*2);

        ref var hover = ref EcsStatic.GetPool<ECMouseHoverable>().Add(ent);
        hover.radius = col.size.x;


        EcsStatic.GetPool<ECTouchDamage>().Add(ent).dps = 10;

        HealthPipe.BuildHealth(ent, max: health);
        return ent;
    }
}
