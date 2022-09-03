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
        [RuntimeInitializeOnLoadMethod]
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
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                    pipelines.Add((ISpawnPipeline)GetScriptableObject(type)); 
                else
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
        static string ResourceFolder = "SpawnPipelines";
        static ScriptableObject GetScriptableObject(Type type)
        {
            var obj = (ScriptableObject)ScriptableObject.FindObjectOfType(type);
            if (obj == null)
                obj = Resources.Load<ScriptableObject>(ResourceFolder +"/" + type.Name);
            if (obj == null)
                throw new Exception($"No Scriptable Object instance found for {type.Name}");
            return obj;
        }
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void BuildScriptableObjects()
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/"+ResourceFolder))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", ResourceFolder);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (typeof(ISpawnPipeline).IsAssignableFrom(type) && typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        EnsureAsset(type);
                    }
        }
        static void EnsureAsset(Type type)
        {
            ScriptableObject obj = Resources.Load<ScriptableObject>(ResourceFolder+"/"+type.Name);
            if (obj == null)
            {
                obj = ScriptableObject.CreateInstance(type);
                UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/Resources/" + ResourceFolder+"/" + type.Name + ".asset");
            }
        }
#endif
    }
}