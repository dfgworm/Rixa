using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;

using MyEcs.Health;
using MyEcs.Spawn;
using MyEcs.Physics;


namespace MyEcs.Actions
{
    public class ProjectileSP : ScriptableObject, ISpawnPipeline
    {
        public void Spawn(EcsWorld world, int ent)
        {
            //expects ProjectileBaggage to have unloaded already

            //world.GetPool<ECNetAutoSpawn>().Add(ent);
            //if some projectiles have a very long lifespan
            // it will cause problems with players joining midgame, unless handled explicitly
            // i can specifically detect and handle such projs, criteria are simple: long life and slow movespeed

            PositionPipe.BuildPosition(world, ent);

            ref var col = ref world.GetPool<ECCollider>().Add(ent);
            col.type = ColliderType.circle;
            col.size = new Vector2(0.3f, 0);

            ref var mesh = ref world.GetPool<ECRenderMesh>().Add(ent);
            mesh.meshId = (int)MeshService.basicMesh.Sphere;
            mesh.rotation = Quaternion.identity;
            mesh.scale = new Vector3(col.size.x, col.size.x, col.size.x);

            if (world.GetPool<ECProjectile>().Get(ent).ownerEntity.Unpack(world, out int ownerEnt))
                world.GetPool<ECCollisionHashFilter>().SoftAdd(ent).filter.Add(ownerEnt);

        }
        public void Destroy(EcsWorld world, int ent)
        {
            
        }
    }

    public class ProjectileBaggage : IBaggageAutoUnload
    {
        public bool selfDestruct;
        public float damage;
        public float lifetime;
        public EcsPackedEntity ownerEntity;
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECProjectile>().Add(ent).ownerEntity = ownerEntity;
            world.GetPool<ECDamageOnCollision>().Add(ent).damage = damage;
            if (selfDestruct)
                world.GetPool<ECSelfDestructOnCollision>().Add(ent);
            else
                world.GetPool<ECLimitedCollision>().Add(ent);
            world.GetPool<ECDestroyDelayed>().Add(ent).time = lifetime;
        }
    }

    public class VelocityBaggage : IBaggageAutoUnload
    {
        public Vector2 velocity;
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECVelocity>().SoftAdd(ent).velocity = velocity;
        }
    }
    public struct ECProjectile
    {
        public EcsPackedEntity ownerEntity;
    }
}