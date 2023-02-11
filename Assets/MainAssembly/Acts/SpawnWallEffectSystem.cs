using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Act
{
    public class SpawnWallEffectSystem : IEcsRunSystem
    {

        readonly EcsFilterInject<Inc<AMPointHit, ACSpawnWall>> pointHitFilter = "act";

        public void Run(IEcsSystems systems)
        {

            foreach (int i in pointHitFilter.Value)
                foreach (var point in pointHitFilter.Pools.Inc1.Get(i).points)
                    ProcessHit(i, point, ref pointHitFilter.Pools.Inc2.Get(i));

        }

        void ProcessHit(int ac, Vector2 point, ref ACSpawnWall acWall)
        {
            int wall = WallSP.Spawn(pos: point);
            EcsStatic.GetPool<ECDestroyDelayed>().Add(wall).time = acWall.lifetime;
        }

    }
    public struct ACSpawnWall
    {
        public float lifetime;
    }
}