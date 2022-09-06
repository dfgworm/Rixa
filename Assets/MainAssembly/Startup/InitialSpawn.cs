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
    }
    static void SpawnWall(float x, float y)
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.payload = new BaggagePayload()
            .Add(new PositionBaggage { position = new Vector2(x,y) })
            .Add(SpawnPipelineBaggage.Get<WallSpawnPipeline>())
            ;
    }
}
