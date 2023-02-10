using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Acts
{
    public class SpawnWallEffectSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACSpawnWall> spawnWallPool = "act";

        readonly EcsWorldInject actWorld = "act";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVPointHit>(out var pool))
                ProcessActEffect(ref pool.Get(i));

        }

        void ProcessActEffect(ref AEVPointHit ev)
        {
            if (!ev.act.Unpack(actWorld.Value, out int ac))
                return;
            if (!spawnWallPool.Value.Has(ac))
                return;


            ref var acWall = ref spawnWallPool.Value.Get(ac);
            int wall = WallSP.Spawn(pos: ev.point);
            EcsStatic.GetPool<ECDestroyDelayed>().Add(wall).time = acWall.lifetime;
        }

    }
    public struct ACSpawnWall
    {
        public float lifetime;
    }
}