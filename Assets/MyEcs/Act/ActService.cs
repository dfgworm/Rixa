using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

namespace MyEcs.Acts
{
    public static class ActService
    {
        public static EcsWorld world;
        static bool _init = false;
        public static void Load()
        {
            if (_init)
                return;
            _init = true;

            world = EcsStatic.AddWorld("act");
        }
        public static EcsPool<T> GetPool<T>() where T : struct
        {
            return world.GetPool<T>();
        }
        public static int CreateAct(int ent)
        {
            var world = EcsStatic.world;
            var list = world.GetPool<ECActs>().SoftAdd(ent).acts;
            int ac = ActService.world.NewEntity();
            byte c = (byte)list.Count;
            list.Add(ac);
            ref var acEnt = ref GetPool<ACEntity>().Add(ac);
            acEnt.entity = world.PackEntity(ent);
            acEnt.id = c;
            return ac;
        }
        public static void RemoveAct(int ac)
        {
            var world = EcsStatic.world;
            if (TryGetEntity(ac, out int ent) && world.GetPool<ECActs>().Has(ent))
            {
                var list = world.GetPool<ECActs>().Get(ent).acts;
                list.Remove(ac);
                int c = list.Count;
                for (int i = 0; i < c; i++)
                    ActService.world.GetPool<ACEntity>().Get(list[i]).id = (byte)i;
                if (c == 0)
                    world.GetPool<ECActs>().Del(ent);
            }
            ActService.world.DelEntity(ac);
        }
        public static bool TryGetEntity(int ac, out int ent)
        {
            return GetPool<ACEntity>().Get(ac).entity.Unpack(EcsStatic.world, out ent);
        }
        public static byte GetActId(int ac)
        {
            return GetPool<ACEntity>().Get(ac).id;
        }
        public static bool TryGetAct(int ent, byte id, out int ac)
        {
            ac = -1;
            var pool = EcsStatic.GetPool<ECActs>();
            if (!pool.Has(ent))
                return false;
            ac = pool.Get(ent).acts[id];
            return true;
        }
        public static void Unload()
        {
            if (!_init)
                return;
            _init = false;

            world.Destroy();
            world = null;
        }
    }
    public struct ECActs : IEcsAutoReset<ECActs>
    {
        public List<int> acts;

        public void AutoReset(ref ECActs c)
        {
            if (c.acts == null)
                c.acts = new List<int>(4);
            else
            {
                foreach (int ac in c.acts)
                    ActService.world.DelEntity(ac);
                c.acts.Clear();
            }
        }
    }
    public struct ACEntity
    {
        public byte id;
        public EcsPackedEntity entity;
    }
    public struct AEVUse : IEventReplicant
    {
        public bool netReceived;
        public EcsPackedEntity act;
        public ActTargetContainer target;
    }
    public struct AEVEntityHit : IEventReplicant
    {
        public EcsPackedEntity act;
        public EcsPackedEntity victim;
    }
    public struct AEVPointHit : IEventReplicant
    {
        public EcsPackedEntity act;
        public Vector2 point;
    }
    public enum ActTargetType : byte
    {
        none,
        point,
        direction,
        entity,
    }
    public struct ActTargetContainer
    {
        public ActTargetType type;
        public Vector2 vector;
        public EcsPackedEntity entity;
    }
}