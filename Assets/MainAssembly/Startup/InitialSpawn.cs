using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using MyEcs.Spawn;
using MyEcs.Actions;
using MyEcs.Physics;
using Generation;

public static class InitialSpawn
{
    public static void Spawn()
    {
        SpawnWall(5, 5);
        SpawnWall(5, 0);
        SpawnWall(0, 5);
        SpawnEnemy(0, -5);
        LoadCameraFocus(EcsStatic.world);
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload
            .Add(SpawnPipelineBaggage.Get<PlayerSP>())
            .Add(new PrefabIdBaggage { id = PrefabId.player })
            .Add(new PositionBaggage { position = new Vector2(2, 2) });
        //SpawnChar();
        //GetPrintLoc();
    }
    static void SpawnWall(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload
            .Add(SpawnPipelineBaggage.Get<WallSP>())
            .Add(new PositionBaggage { position = new Vector2(x, y) })
            ;
    }
    static void SpawnEnemy(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload
            .Add(SpawnPipelineBaggage.Get<EnemySP>())
            .Add(new PositionBaggage { position = new Vector2(x, y) })
            ;
    }
    static void LoadCameraFocus(EcsWorld world)
    {
        var cam = GameObject.FindObjectOfType<CameraFocus>();
        if (!cam)
            throw new Exception("No CameraFocus found");
        int ent = world.NewEntity();
        world.GetPool<ECCameraFocus>().Add(ent).focus = cam;
    }
    static void SpawnChar()
    {

        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECPosition>().Add(ent);

        EcsStatic.GetPool<ECLocalControllable>().Add(ent);
        ref var mesh = ref EcsStatic.GetPool<ECRenderMesh>().Add(ent);
        mesh.meshId = (int)MeshService.basicMesh.Cylinder;

        EcsStatic.GetPool<ECVelocity>().Add(ent);
        ref var targetVel = ref EcsStatic.GetPool<ECTargetVelocity>().Add(ent);
        targetVel.acceleration = 50;

        int ac = EcsActionService.CreateAction(ent);
        ref var dash = ref EcsActionService.GetPool<ACDash>().Add(ac);
        dash.range = 6;
        dash.velocity = 30;

        int channelAc = EcsActionService.CreateAction(ent);
        EcsActionService.GetPool<ACInputType>().Add(channelAc).targetType = ActionTargetType.point;
        ref var channel = ref EcsActionService.GetPool<ACChannelled>().Add(channelAc);
        channel.duration = 0.5f;
        channel.finishAction = EcsActionService.acWorld.PackEntity(ac);

        ref var channelDisplay = ref EcsStatic.GetPool<ECChannelDisplay>().Add(ent);
        channelDisplay.Init();
        channelDisplay.controller.shift = new Vector3(0,3,0);

        //EcsActionService.GetPool<ACInstantDelivery>().Add(ac);
        //ref var proj = ref EcsActionService.GetPool<ACProjectileDelivery>().Add(ac);
        //proj.selfDestruct = true;
        //proj.velocity = 5;
        //proj.lifetime = 5;
        //ref var effect = ref EcsActionService.GetPool<ACDamage>().Add(ac);
        //effect.amount = 10;
        //ref var effect = ref EcsActionService.GetPool<ACSpawnWall>().Add(ac);
        //effect.lifetime = 5;

    }
    static void GetPrintLoc()
    {
        var gen = new ShipGenerator();
        var loc = gen.GenerateSmallShip(5, 8,
            extraExitCount: 2
            );

        string[][] strings = new string[loc.Size.y][];
        for (int y = 0; y < loc.Size.y; y++)
            strings[y] = new string[loc.Size.x];
        foreach (Cell c in loc.Cell.GetAllCells())
        {
            var room = loc.GetRoomAt(c);
            string str = room.roomId.ToString() + ((ShipGenerator.RoomType)room.typeId).ToString().PadRight(13);
            strings[c.pos.y][c.pos.x] = str;
        }
        for (int y = 0; y < loc.Size.y; y++)
            Debug.Log(String.Join(":", strings[y]));

    }
}
