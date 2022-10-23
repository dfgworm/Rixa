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
using MyEcs.Physics;
using MyEcs.Net;

public static class InitialSpawn
{
    public static void Spawn()
    {
        SpawnWall(5, 5);
        SpawnWall(5, 0);
        SpawnWall(0, 5);
        LoadCameraFocus(EcsStatic.world);
    }
    static void SpawnWall(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.payload = new BaggagePayload()
            .Add(new PositionBaggage { position = new Vector2(x,y) })
            .Add(SpawnPipelineBaggage.Get<WallSpawnPipeline>())
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
}
