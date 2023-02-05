using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;

namespace MyEcs.Actions
{
    public static class EcsActionService
    {
        public static EcsWorld acWorld;
        static bool _init = false;
        public static void Load()
        {
            if (_init)
                return;
            _init = true;

            acWorld = EcsStatic.AddWorld("actions");
        }
        public static EcsPool<T> GetPool<T>() where T : struct
        {
            return acWorld.GetPool<T>();
        }
        public static int CreateAction(int ent)
        {
            var world = EcsStatic.world;
            var list = world.GetPool<ECActions>().SoftAdd(ent).actions;
            int ac = acWorld.NewEntity();
            byte c = (byte)list.Count;
            list.Add(ac);
            ref var acEnt = ref GetPool<ACEntity>().Add(ac);
            acEnt.entity = world.PackEntity(ent);
            acEnt.id = c;
            return ac;
        }
        public static void RemoveAction(int ac)
        {
            var world = EcsStatic.world;
            if (TryGetEntity(ac, out int ent) && world.GetPool<ECActions>().Has(ent))
            {
                var list = world.GetPool<ECActions>().Get(ent).actions;
                list.Remove(ac);
                int c = list.Count;
                for (int i = 0; i < c; i++)
                    acWorld.GetPool<ACEntity>().Get(list[i]).id = (byte)i;
                if (c == 0)
                    world.GetPool<ECActions>().Del(ent);
            }
            acWorld.DelEntity(ac);
        }
        public static bool TryGetEntity(int ac, out int ent)
        {
            return GetPool<ACEntity>().Get(ac).entity.Unpack(EcsStatic.world, out ent);
        }
        public static byte GetActionId(int ac)
        {
            return GetPool<ACEntity>().Get(ac).id;
        }
        public static bool TryGetAction(int ent, byte id, out int ac)
        {
            ac = -1;
            var pool = EcsStatic.GetPool<ECActions>();
            if (!pool.Has(ent))
                return false;
            ac = pool.Get(ent).actions[id];
            return true;
        }
        public static void Unload()
        {
            if (!_init)
                return;
            _init = false;

            acWorld.Destroy();
            acWorld = null;
        }
    }
    public struct ECActions : IEcsAutoReset<ECActions>
    {
        public List<int> actions;

        public void AutoReset(ref ECActions c)
        {
            if (c.actions == null)
                c.actions = new List<int>(4);
            else
            {
                foreach (int ac in c.actions)
                    EcsActionService.acWorld.DelEntity(ac);
                c.actions.Clear();
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
        public EcsPackedEntity action;
        public ActionTargetContainer target;
    }
    public struct AEVEntityHit : IEventReplicant
    {
        public EcsPackedEntity action;
        public EcsPackedEntity victim;
    }
    public struct AEVPointHit : IEventReplicant
    {
        public EcsPackedEntity action;
        public Vector2 point;
    }
    public enum ActionTargetType : byte
    {
        none,
        point,
        direction,
        entity,
    }
    public struct ActionTargetContainer
    {
        public ActionTargetType type;
        public Vector2 vector;
        public EcsPackedEntity entity;
    }
}