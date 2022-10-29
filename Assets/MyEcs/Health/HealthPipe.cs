using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using Mirror;
using MyEcs.Net;
using MyEcs.Spawn;

namespace MyEcs.Health
{
    public static class HealthPipe
    {
        public struct HealthArgs {
            public float max;
            public float percent;
            public float regen;
        }

        public static void BuildHealth(EcsWorld world, int ent, HealthArgs args)
        {
            ref var hp = ref world.GetPool<ECHealth>().Add(ent);
            hp.max = args.max;
            hp.Percent = args.percent;
            if (args.regen > 0)
                world.GetPool<ECHealthRegen>().Add(ent).regen = args.regen;
            ref var hpDisp = ref world.GetPool<ECHealthDisplay>().Add(ent);
            hpDisp.Init();
            world.GetPool<EMSpawned>().Get(ent).payload.EnsureBaggage<HealthBaggage>(world, ent);
        }
        public static void BuildNetHealth(EcsWorld world, int ent)
        {
            if (ECNetOwner.IsEntityLocalAuthority(world, ent))
                BuildHealthLocalAuthority(world, ent);
            else
                BuildHealthRemoteAuthority(world, ent);
        }
        static void BuildHealthLocalAuthority(EcsWorld world, int ent)
        {
            ref var send = ref world.GetPool<ECSyncSend>().SoftAdd(ent);
            send.Payload.Add(new HealthBaggage());
        }
        static void BuildHealthRemoteAuthority(EcsWorld world, int ent)
        {
            world.GetPool<ECSyncReceive>().SoftAdd(ent);
            if (NetStatic.IsServer)
            {
                ref var netOwner = ref world.GetPool<ECNetOwner>().Get(ent);
                var pb = PlayerBehaviourBase.GetById(netOwner.playerId);
                world.GetPool<ECSyncSend>().SoftAdd(ent)
                    .exceptionConnectionId = pb.connection.connectionId;
            }
        }
    }

    public class HealthBaggage : IUpdatableBaggage
    {
        public float health;
        public bool IsUpToDate(EcsWorld world, int ent)
        {
            return world.GetPool<ECHealth>().Get(ent).Current == health;
        }
        public void LoadToBaggage(EcsWorld world, int ent)
        {
            var pool = world.GetPool<ECHealth>();
            health = pool.Get(ent).Current;
        }
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECHealth>().Get(ent).Current = health;
        }
    }
}