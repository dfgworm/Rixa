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
using MyEcs.Net;
using Generation;

public static class InitialSpawn
{
    public static void Spawn()
    {
        if (NetStatic.IsServer) {
            SpawnWall(5, 5);
            SpawnWall(5, 0);
            SpawnWall(0, 5);
            SpawnEnemy(0, -5);
        }
        LoadCameraFocus(EcsStatic.world);
        var gen = new ShipGenerator();
        var loc = gen.GenerateSmallShip(5,8,
            extraExitCount:2
            );

        string[][] strings = new string[loc.Size.y][];
        for (int y = 0; y < loc.Size.y; y++)
            strings[y] = new string[loc.Size.x];
        foreach (Cell c in loc.Cell.GetAllCells())
        {
            var room = loc.GetRoomAt(c);
            string str = room.roomId.ToString()+((ShipGenerator.RoomType)room.typeId).ToString().PadRight(13);
            strings[c.pos.y][c.pos.x] = str;
        }
        for (int y = 0; y < loc.Size.y; y++)
            Debug.Log(String.Join(":", strings[y]));
        //SpawnChar();
    }
    static void SpawnWall(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload
            .Add(SpawnPipelineBaggage.Get<WallSP>())
            .Add(new PositionBaggage { position = new Vector2(x, y) })
            .Add(NetIdBaggage.Allocate())
            ;
    }
    static void SpawnEnemy(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.Payload
            .Add(SpawnPipelineBaggage.Get<EnemySP>())
            .Add(new PositionBaggage { position = new Vector2(x, y) })
            .Add(NetIdBaggage.Allocate())
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

        int ac = EcsActionService.CreateAction(ent);
        EcsActionService.GetPool<ACLocalControllable>().Add(ac);
        ref var proj = ref EcsActionService.GetPool<ACProjectileLaunch>().Add(ac);
        proj.damage = 10;
        proj.selfDestruct = true;
        proj.velocity = 5;

    }
}
