using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


public class RenderSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    public const int instanceCountLimit = 1023;

    readonly EcsFilterInject<Inc<ECRenderMesh, ECPosition>> renderFilter = default;

    class InstancedData
    {
        public int drawCount = 0;
        public Matrix4x4[] matrices = new Matrix4x4[instanceCountLimit];
    }
    Dictionary<int, InstancedData> dataDictionary;
    public void Init(IEcsSystems systems)
    {
        dataDictionary = new Dictionary<int, InstancedData>(256);
    }

    public void Run(IEcsSystems systems)
    {
        foreach (int i in renderFilter.Value)
            ProcessMesh(
                ref renderFilter.Pools.Inc1.Get(i),
                ref renderFilter.Pools.Inc2.Get(i)
                );
        for (int meshId = 0; meshId < MeshService.meshInfos.Count; meshId++)
            if (dataDictionary.ContainsKey(meshId))
                DispatchInstancedRender(meshId);
    }
    InstancedData GetMeshData(int meshId)
    {
        if (!dataDictionary.ContainsKey(meshId))
            dataDictionary[meshId] = new InstancedData();
        return dataDictionary[meshId];
    }
    void ProcessMesh(ref ECRenderMesh mesh, ref ECPosition pos)
    {
        var data = GetMeshData(mesh.meshId);
        if (data.drawCount >= instanceCountLimit)
            DispatchInstancedRender(mesh.meshId);
        data.matrices[data.drawCount].SetTRS(
            pos.position2.Vec3() + mesh.offset,
            mesh.rotation, mesh.scale);
        data.drawCount += 1;
    }
    void DispatchInstancedRender(int meshId)
    {
        var data = GetMeshData(meshId);
        if (data.drawCount == 0)
            return;
        var meshInfo = MeshService.GetById(meshId);
        Graphics.DrawMeshInstanced(meshInfo.mesh, 0, meshInfo.renderParams.material,
            data.matrices, data.drawCount, meshInfo.renderParams.matProps);
        data.drawCount = 0;
    }

    public void Destroy(IEcsSystems systems)
    {
        dataDictionary = null;
    }
}
public struct ECRenderMesh : IEcsAutoReset<ECRenderMesh>
{
    public Vector3 offset;
    public Quaternion rotation;
    public Vector3 scale;
    public int meshId;

    public void AutoReset(ref ECRenderMesh c)
    {
        c.offset = Vector3.zero;
        c.rotation = Quaternion.identity;
        c.scale = Vector3.one;
    }
}
