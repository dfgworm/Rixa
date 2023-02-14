using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Physics;
using MyEcs.Health;
using MyEcs.Act;

public enum EnemyType {
    Shooter,
    Swordsman,
    Rushdown,
    Shielder,
}
public static class EnemySP
{
    public static float health = 100;
    public static float moveSpeed = 2f;
    public static int Spawn(EnemyType type)
    {
        int entInt = EcsStatic.world.NewEntity();
        EcsEntity ent = EcsStatic.GetEntity(entInt);
        TeamService.SetTeam(entInt, Team.enemy);

        EcsStatic.GetPool<ECPosition>().Add(entInt);
        EcsStatic.GetPool<ECPositionToTransform>().Add(entInt);

        ref var acc = ref EcsStatic.GetPool<ECDesiredVelocity>().Add(entInt);
        acc.targetSpeed = moveSpeed;
        acc.acceleration = 100;
        EcsStatic.GetPool<ECVelocity>().Add(entInt);
        EcsStatic.GetPool<ECRespectObstacles>().Add(entInt);

        var model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        EcsGameObjectService.Link(entInt, model);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(entInt);
        col.type = ColliderType.circle;
        col.size = new Vector2(1, 0);
        model.transform.localScale = new Vector3(col.size.x*2, 1, col.size.x*2);

        ref var hover = ref EcsStatic.GetPool<ECMouseHoverable>().Add(entInt);
        hover.radius = col.size.x;


        EcsStatic.GetPool<ECTouchDamage>().Add(entInt).dps = 10;

        EcsStatic.GetPool<ECSearchTarget>().Add(entInt).distance = 10;

        if (type == EnemyType.Shooter)
        {
            AIShooterPipe.BuildEntity(ent, new FloatRange(7, 15));
            var act = GiveShot(entInt);
            AIShooterPipe.BuildAct(act, new FloatRange(5,20));
        }

        HealthPipe.BuildHealth(entInt, max: health);

        return entInt;
    }
    static EcsEntity GiveShot(int ent)
    {
        EcsEntity acEnt = ActService.CreateActStruct(ent);
        ref var proj = ref acEnt.Add<ACProjectileDelivery>();
        proj.lifetime = 2;
        proj.selfDestruct = true;
        proj.velocity = 20;
        ref var dmg = ref acEnt.Add<ACDamage>();
        dmg.amount = 20;

        ref var ammo = ref acEnt.Add<ACAmmo>();
        ammo.amount.max = 1;
        ammo.amount.Current = 1;
        acEnt.Add<ACAmmoRegen>().Cooldown = 2;
        
        return acEnt;
    }
}
