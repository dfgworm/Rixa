using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyEcs.Spawn
{
    public class UnitySpawnPipelineManager : ScriptableObject
    {
        private const string STORAGE_PATH = "SpawnPipelines/"+nameof(UnitySpawnPipelineManager);
        private static Dictionary<int, ResourceAsset> assetsDict;
#if UNITY_EDITOR
        //[UnityEditor.InitializeOnLoadMethod]
        private static void BuildMap()
        {
            var index = Resources.Load<UnitySpawnPipelineManager>(STORAGE_PATH);
            if (index == null)
            {
                index = CreateInstance<UnitySpawnPipelineManager>();
                UnityEditor.AssetDatabase.CreateAsset(index, "Assets/Resources/" + STORAGE_PATH + ".asset");
            }
            index.assets = new List<ResourceAsset>();



            UnityEditor.EditorUtility.SetDirty(index);
        }
        private void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false)
                BuildMap();
        }
#endif
        //[RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            var index = Resources.Load<UnitySpawnPipelineManager>(STORAGE_PATH);
            assetsDict = new Dictionary<int, ResourceAsset>();
            for (int i = 0; i < index.assets.Count; i++)
            {
                ResourceAsset asset = index.assets[i];
                if (assetsDict.ContainsKey(asset.id))
                {
                    Debug.LogErrorFormat("Duplicate asset ids = {0}", asset.id);
                    continue;
                }
                assetsDict.Add(asset.id, asset);
            }
        }

        public static T GetAsset<T>(int id) where T : Object
        {
            ResourceAsset asset;
            if (assetsDict.TryGetValue(id, out asset))
                return Resources.Load<T>(asset.assetPath);
            return null;
        }
        [System.Serializable]
        public struct ResourceAsset
        {
            public int id;
            public string assetPath;
        }
        public List<ResourceAsset> assets;
    }
}