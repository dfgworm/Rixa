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

            var proj = world.GetPool<ECProjectile>().Get(ent);

            if (!proj.sourceAction.Unpack(EcsActionService.acWorld, out int ac))
                throw new Exception("No action found for projectile");

            ref var acProj = ref EcsActionService.GetPool<ACProjectileDelivery>().Get(ac);

            if (acProj.selfDestruct)
                world.GetPool<ECSelfDestructOnCollision>().Add(ent);
            else
                world.GetPool<ECLimitedCollision>().Add(ent);
            //world.GetPool<ECDestroyDelayed>().Add(ent).time = acProj.lifetime;
            //world.GetPool<ECVelocity>().Add(ent).velocity = acProj.velocity; //also need to supply direction here, decided to use velocityBaggage for now

            if (proj.ownerEntity.Unpack(world, out int ownerEnt))
                world.GetPool<ECCollisionHashFilter>().SoftAdd(ent).filter.Add(ownerEnt);

        }
        public void Destroy(EcsWorld world, int ent)
        {
            
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
    public class LifetimeBaggage : IBaggageAutoUnload
    {
        public float lifetime;
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECDestroyDelayed>().SoftAdd(ent).time = lifetime;
        }

    }
    public class ProjectileBaggage : IBaggageAutoUnload
    {
        public EcsPackedEntity ownerEntity;
        public EcsPackedEntity sourceAction;
        public void UnloadToWorld(EcsWorld world, int ent)
        {
            ref var proj = ref world.GetPool<ECProjectile>().Add(ent);
            proj.ownerEntity = ownerEntity;
            proj.sourceAction = sourceAction;
        }
    }
}