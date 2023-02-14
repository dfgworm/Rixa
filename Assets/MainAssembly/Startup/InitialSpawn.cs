using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using MyEcs.Act;
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
        LoadCameraFocus();
        SpawnChar(2,2);
        //GetPrintLoc();
    }
    static void SpawnChar(float x, float y)
    {
        int ent = PlayerSP.Spawn();
        EcsStatic.GetPool<ECPosition>().Get(ent).position2 = new Vector2(x, y);
    }
    static void SpawnWall(float x, float y)
    {
        WallSP.Spawn(pos: new Vector2(x, y));
    }
    static void SpawnEnemy(float x, float y)
    {
        int ent = EnemySP.Spawn(EnemyType.Shooter);
        EcsStatic.GetPool<ECPosition>().Get(ent).position2 = new Vector2(x, y);
    }
    static void LoadCameraFocus()
    {
        var cam = GameObject.FindObjectOfType<CameraFocus>();
        if (!cam)
            throw new Exception("No CameraFocus found");
        int ent = EcsStatic.world.NewEntity();
        EcsStatic.GetPool<ECCameraFocus>().Add(ent).focus = cam;
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
