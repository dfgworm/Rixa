using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;


public static class MeshService
{

    public enum basicMesh
    {
        Cube,
        Sphere,
        Cylinder,
    }

    public class MeshInfo
    {
        public RenderParams renderParams;
        public Mesh mesh;
        public MeshInfo()
        {
            mesh = null;
            renderParams = new RenderParams();
        }
    }
    public static List<MeshInfo> meshInfos;
    public static void Load()
    {
        meshInfos = new List<MeshInfo>(256);

        Register(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), Resources.Load<Material>("Materials/DefaultMaterial"));
        Register(Resources.GetBuiltinResource<Mesh>("Sphere.fbx"), Resources.Load<Material>("Materials/DefaultMaterial"));
        Register(Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"), Resources.Load<Material>("Materials/DefaultMaterial"));

    }
    public static MeshInfo GetById(int id)
    {
        return meshInfos[id];
    }
    static void Register(Mesh mesh, Material material)
    {
        var container = new MeshInfo();
        container.mesh = mesh;
        container.renderParams.material = material;
        meshInfos.Add(container);
    }

    public static void Unload()
    {
        foreach(var container in meshInfos)
        {
            Resources.UnloadAsset(container.mesh);
            Resources.UnloadAsset(container.renderParams.material);
        }
        meshInfos = null;
    }
}
