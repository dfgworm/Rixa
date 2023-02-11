using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Health;
using MyEcs.Physics;


namespace MyEcs.Act
{
    public static class ProjectileSP
    {
        public static int Spawn(int act, Vector2 pos, Vector2 dir)
        {
            int ent = EcsStatic.world.NewEntity();
            EcsStatic.GetPool<ECPosition>().Add(ent).position2 = pos;

            ref var col = ref EcsStatic.GetPool<ECCollider>().Add(ent);
            col.type = ColliderType.circle;
            col.size = new Vector2(0.3f, 0);

            ref var mesh = ref EcsStatic.GetPool<ECRenderMesh>().Add(ent);
            mesh.meshId = (int)MeshService.basicMesh.Sphere;
            mesh.rotation = Quaternion.identity;
            mesh.scale = new Vector3(col.size.x, col.size.x, col.size.x);

            ref var proj = ref EcsStatic.GetPool<ECProjectile>().Add(ent);
            proj.sourceAct = ActService.world.PackEntity(act);

            ref var acProj = ref ActService.GetPool<ACProjectileDelivery>().Get(act);

            if (acProj.selfDestruct)
                EcsStatic.GetPool<ECSelfDestructOnCollision>().Add(ent);
            else
                EcsStatic.GetPool<ECLimitedCollision>().Add(ent);
            EcsStatic.GetPool<ECVelocity>().Add(ent).velocity = dir*acProj.velocity;
            EcsStatic.GetPool<ECDestroyDelayed>().SafeAdd(ent).time = acProj.lifetime;

            int owner = ActService.GetEntity(act);
            EcsStatic.GetPool<ECCollisionHashFilter>().SafeAdd(ent).filter.Add(owner);
            return ent;
        }
    }
}