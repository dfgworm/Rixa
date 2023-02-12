using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Physics;
using MyEcs.Health;
using MyEcs.Act;

public static class PlayerSP
{
    public static float moveSpeed = 10f;
    public static float collisionSize = 1f;
    public static float health = 100;
    public static float regen = 1;

    public static float syncPeriodFromClient = 1 / 20f;
    public static float syncPeriodFromServer = 1 / 20f;

    public static int Spawn()
    {
        int ent = EcsStatic.world.NewEntity();
        TeamService.SetTeam(ent, Team.player);
        EcsStatic.GetPool<ECPosition>().Add(ent);
        EcsStatic.GetPool<ECPositionToTransform>().Add(ent);
        HealthPipe.BuildHealth(ent, max: health, regen: regen);

        EcsStatic.GetPool<ECLocalControllable>().Add(ent);

        ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
        col.type = ColliderType.circle;
        col.size = new Vector2(collisionSize, 0);

        var go = PrefabRegistry.CreateForEntity(ent, PrefabId.player);
        go.transform.localScale = new Vector3(col.size.x * 2, 1, col.size.x * 2);


        ref var acc = ref EcsStatic.GetPool<ECDesiredVelocity>().Add(ent);
        acc.targetSpeed = moveSpeed;
        acc.acceleration = 100;
        EcsStatic.GetPool<ECVelocity>().Add(ent);
        EcsStatic.GetPool<ECRespectObstacles>().Add(ent);

        EcsStatic.GetPool<ECPotentialTarget>().Add(ent);
        GiveDash(ent);
        GiveShot(ent);
        GiveSlash(ent);
        return ent;
    }
    static void GiveDash(int ent)
    {

        int acEnt = ActService.CreateAct(ent);
        ref var dash = ref ActService.GetPool<ACDash>().Add(acEnt);
        dash.range = 8;
        dash.velocity = 16;
        dash.maxForce = 40;
        ActService.GetPool<ACInputType>().Add(acEnt).targetType = ActTargetType.point;
        PlayerInputSystem.ConnectActToInput(PlayerInputSystem.controls.Player.Dash, acEnt);
    }
    static void GiveShot(int ent)
    {

        int acEnt = ActService.CreateAct(ent);
        ref var proj = ref ActService.GetPool<ACProjectileDelivery>().Add(acEnt);
        proj.lifetime = 5;
        proj.selfDestruct = true;
        proj.velocity = 20;
        ref var dmg = ref ActService.GetPool<ACDamage>().Add(acEnt);
        dmg.amount = 20;
        ActService.GetPool<ACInputType>().Add(acEnt).targetType = ActTargetType.direction;
        PlayerInputSystem.ConnectActToInput(PlayerInputSystem.controls.Player.SecondaryAttack, acEnt);

        ref var ammo = ref ActService.GetPool<ACAmmo>().Add(acEnt);
        ammo.amount.max = 3;
        ammo.amount.Current = 3;
        ActService.GetPool<ACAmmoRegen>().Add(acEnt).Cooldown = 1;
    }
    static void GiveSlash(int ent)
    {

        int acEnt = ActService.CreateAct(ent);
        ref var area = ref ActService.GetPool<ACAreaDelivery>().Add(acEnt);
        area.areaType = AreaType.concave;
        area.radius = 4;
        area.exclusiveRadius = collisionSize;
        area.concaveAngle = Mathf.PI * 0.4f;
        area.teamFilter = Team.enemy;
        ref var dmg = ref ActService.GetPool<ACDamage>().Add(acEnt);
        dmg.amount = 50;
        ActService.GetPool<ACInputType>().Add(acEnt).targetType = ActTargetType.point;
        PlayerInputSystem.ConnectActToInput(PlayerInputSystem.controls.Player.PrimaryAttack, acEnt);
    }
}
