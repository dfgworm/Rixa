using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Spawn;

namespace MyEcs.Actions
{
    public class SpawnWallEffectSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<ACSpawnWall> spawnWallPool = "actions";

        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;


        public void Run(IEcsSystems systems)
        {
            foreach (int i in bus.Value.GetEventBodies<AEVPointHit>(out var pool))
                ProcessActionEffect(ref pool.Get(i));

        }

        void ProcessActionEffect(ref AEVPointHit ev)
        {
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!spawnWallPool.Value.Has(ac))
                return;


            ref var acWall = ref spawnWallPool.Value.Get(ac);
            ref var spawnEv = ref bus.Value.NewEvent<EVSpawn>();
            spawnEv.Payload
                .Add(SpawnPipelineBaggage.Get<WallSP>())
                .Add(new PositionBaggage { position = ev.point })
                .Add(new LifetimeBaggage { lifetime = acWall.lifetime })
                ;
        }

    }
    public struct ACSpawnWall
    {
        public float lifetime;
    }
}