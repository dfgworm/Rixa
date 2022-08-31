using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Reflection;

namespace MyEcs.Spawn
{
    public static class SpawnPipelineIdService
    {

        static Dictionary<Type, byte> typeToId;
        static List<Type> idToType;
        static List<ISpawnPipeline> pipelines;
        static bool _init = false;
        public static void Init()
        {
            if (_init)
                return;
            _init = true;
            typeToId = new Dictionary<Type, byte>();
            pipelines = new List<ISpawnPipeline>();
            idToType = TypeSerializer.ProduceIdList(typeof(ISpawnPipeline));
            for (byte i = 0; i < idToType.Count; i++)
            {
                var type = idToType[i];
                typeToId.Add(type, i);
                pipelines.Add((ISpawnPipeline)Activator.CreateInstance(type));
            }
        }
        public static Type GetType(byte id)
        {
            return idToType[id];
        }
        public static byte GetObjectId(object obj)
        {
            return GetTypeId(obj.GetType());
        }
        public static byte GetTypeId(Type type)
        {
#if DEBUG
            if (!typeToId.ContainsKey(type))
                throw new Exception(type.Name+" is not a ISpawnPipeline");
#endif
            return typeToId[type];
        }
        public static ISpawnPipeline GetPipeline(byte id)
        {
            if (id == 0b_1111_1111)
                throw new Exception("invalid pipeline id provided");
            return pipelines[id];
        }
        public static ISpawnPipeline GetPipeline(Type type)
        {
            return GetPipeline(GetTypeId(type));
        }
        public static T GetPipeline<T>()
            where T : ISpawnPipeline
        {
            return (T)GetPipeline(GetTypeId(typeof(T)));
        }
    }
}