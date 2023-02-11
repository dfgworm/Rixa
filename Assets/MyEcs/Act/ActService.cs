using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

namespace MyEcs.Act
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
            var list = world.GetPool<ECActs>().SafeAdd(ent).acts;
            int ac = ActService.world.NewEntity();
            byte c = (byte)list.Count;
            list.Add(ac);
            ref var acEnt = ref GetPool<ACEntity>().Add(ac);
            acEnt.entity = ent;
            acEnt.id = c;
            return ac;
        }
        public static void RemoveAct(int ac)
        {
            var world = EcsStatic.world;
            int ent = GetEntity(ac);
            if (world.GetPool<ECActs>().Has(ent))
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
        public static int GetEntity(int ac)
        {
            return GetPool<ACEntity>().Get(ac).entity;
        }
        public static byte GetActId(int ac) //Act id is completely unreliable and might change if entity's act list changes
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
        public int entity;
    }
    public struct AMUsed : IEcsAutoReset<AMUsed>
    {
        public List<ActUsageContainer> usages;

        public void AutoReset(ref AMUsed c)
        {
            if (c.usages == null)
                c.usages = new List<ActUsageContainer>(2);
            else
                c.usages.Clear();
        }
    }
    public struct AMEntityHit : IEcsAutoReset<AMEntityHit>
    {
        public List<EcsPackedEntity> victims;
        public void AutoReset(ref AMEntityHit c)
        {
            if (c.victims == null)
                c.victims = new List<EcsPackedEntity>(2);
            else
                c.victims.Clear();
        }
    }
    public struct AMPointHit : IEcsAutoReset<AMPointHit>
    {
        public List<Vector2> points;
        public void AutoReset(ref AMPointHit c)
        {
            if (c.points == null)
                c.points = new List<Vector2>(2);
            else
                c.points.Clear();
        }
    }
    public enum ActTargetType : byte
    {
        none,
        point,
        direction,
        entity,
    }
    public struct ActUsageContainer
    {
        public ActTargetType targetType;
        public Vector2 vector;
        public EcsPackedEntity entity;
    }
}