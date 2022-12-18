using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mirror;
using Leopotam.EcsLite;


namespace MyEcs.Net
{
    public static class NetIdService
    {
        private static Dictionary<ushort, EcsPackedEntityWithWorld> netEntities = new Dictionary<ushort, EcsPackedEntityWithWorld>();
        private static ushort nextId = 1;
        //[Server]
        public static ushort AllocateNetId()
        {
            ushort id = nextId;
            nextId++;
            return id;
        }
        public static void RegisterEntity(int ent, ushort id) => RegisterEntity(EcsStatic.world, ent, id);
        public static void RegisterEntity(EcsWorld world, int ent, ushort id)
        {
            world.GetPool<ECNetId>().Add(ent).id = id;
            netEntities[id] = world.PackEntityWithWorld(ent);
        }
        public static ushort GetNetId(int ent) => GetNetId(EcsStatic.world, ent);
        public static ushort GetNetId(EcsWorld world, int ent)
        {
            TryGetNetId(world, ent, out ushort netId);
            return netId;
        }
        public static bool TryGetNetId(EcsPackedEntity packed, out ushort netId)
        {
            if (!packed.Unpack(EcsStatic.world, out int ent))
            {
                netId = 0;
                return false;
            }
            return TryGetNetId(EcsStatic.world, ent, out netId);
        }
        public static bool TryGetNetId(int ent, out ushort netId) => TryGetNetId(EcsStatic.world, ent, out netId);
        public static bool TryGetNetId(EcsWorld world, int ent, out ushort netId)
        {
            netId = 0;
            if (ent < 0)
                return false;
            var idPool = world.GetPool<ECNetId>();
            if (!idPool.Has(ent))
                return false;
            netId = idPool.Get(ent).id;
            return true;
        }
        public static int GetEntity(ushort netId)
        {
            TryGetEntity(netId, out int ent);
            return ent;
        }
        public static bool TryGetEntity(ushort id, out int ent)
        {
            if (netEntities.TryGetValue(id, out EcsPackedEntityWithWorld packed))
                if (packed.Unpack(out EcsWorld world, out ent))
                    return true;
            ent = -1;
            return false;
        }
        //[Server]
        public static bool IsNetIdUsed(ushort netId)
        {
            return netId > 0 && netId < nextId;
        }
        public static bool HasEntity(ushort netId)
        {
            return netEntities.ContainsKey(netId);
        }
        public static void UnregisterEntity(int ent) => UnregisterEntity(EcsStatic.world, ent);
        public static void UnregisterEntity(EcsWorld world, int ent)
        {
            var pool = world.GetPool<ECNetId>();
            if (pool.Has(ent))
            {
                UnregisterNetId(pool.Get(ent).id);
                world.GetPool<ECNetId>().Del(ent);
            }
        }
        public static void UnregisterNetId(ushort id)
        {
            netEntities.Remove(id);
        }
    }
    public struct ECNetId
    {
        public ushort id;
    }
    public class NetIdBaggage : MyEcs.Spawn.IBaggageAutoUnload
    {
        public ushort id;
        public NetIdBaggage()
        {
        }
        public NetIdBaggage(ushort _id)
        {
            id = _id;
        }
        public static NetIdBaggage Allocate()
        {
            var baggage = new NetIdBaggage();
            baggage.id = NetIdService.AllocateNetId();
            return baggage;
        }

        public void UnloadToWorld(EcsWorld world, int ent)
        {
            NetIdService.RegisterEntity(ent, id);
        }
    }
}