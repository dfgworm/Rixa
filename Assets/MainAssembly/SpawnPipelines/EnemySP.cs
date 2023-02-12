using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Physics;
using MyEcs.Health;
using MyEcs.Act;

public static class EnemySP
{
    public static float health = 100;
    public static float moveSpeed = 2f;
    public static int Spawn()
    {
        int ent = EcsStatic.world.NewEntity();
        TeamService.SetTeam(ent, Team.enemy);

        EcsStatic.GetPool<ECPosition>().Add(ent);
        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);

        ref var acc = ref EcsStatic.GetPool<ECDesiredVelocity>().Add(ent);
        acc.targetSpeed = moveSpeed;
        acc.acceleration = 100;
        EcsStatic.GetPool<ECVelocity>().Add(ent);
        EcsStatic.GetPool<ECRespectObstacles>().Add(ent);

        var model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        EcsGameObjectService.Link(ent, model);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(1, 0);
        model.transform.localScale = new Vector3(col.size.x*2, 1, col.size.x*2);

        ref var hover = ref EcsStatic.GetPool<ECMouseHoverable>().Add(ent);
        hover.radius = col.size.x;

        ref var follower = ref EcsStatic.GetPool<ECFollowTargetAI>().Add(ent);
        follower.minDistance = 5;
        follower.maxDistance = 15;

        EcsStatic.GetPool<ECTouchDamage>().Add(ent).dps = 10;

        EcsStatic.GetPool<ECTarget>().Add(ent);
        EcsStatic.GetPool<ECSearchTarget>().Add(ent).distance = 10;

        HealthPipe.BuildHealth(ent, max: health);

        GiveShot(ent);
        return ent;
    }
    static void GiveShot(int ent)
    {
        int acEnt = ActService.CreateAct(ent);
        ref var proj = ref ActService.GetPool<ACProjectileDelivery>().Add(acEnt);
        proj.lifetime = 2;
        proj.selfDestruct = true;
        proj.velocity = 20;
        ref var dmg = ref ActService.GetPool<ACDamage>().Add(acEnt);
        dmg.amount = 20;

        ref var ammo = ref ActService.GetPool<ACAmmo>().Add(acEnt);
        ammo.amount.max = 1;
        ammo.amount.Current = 1;
        ActService.GetPool<ACAmmoRegen>().Add(acEnt).Cooldown = 2;
        
        ref var attack = ref ActService.world.GetPool<ACAttackTargetAI>().Add(acEnt);
        attack.targetType = ActTargetType.point;
        attack.minDistance = 2;
        attack.maxDistance = 20;
    }
}
