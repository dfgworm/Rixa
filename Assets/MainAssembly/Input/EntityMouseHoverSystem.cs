using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class EntityMouseHoverSystem : IEcsRunSystem
{
    readonly EcsWorldInject world = default;

    readonly EcsFilterInject<Inc<ECPosition, ECMouseHoverable>> hoverFilter = default;

    readonly EcsCustomInject<EventBus> bus = default;

    int currentEntity = -1;
    float closestDistance = -1;
    public void Run(IEcsSystems systems)
    {
        if (Camera.main == null)
            return;
        var mousePos = bus.Value.GetEventBodySingleton<InpMousePosition>().pos;
        var ray = Camera.main.ScreenPointToRay(mousePos);
        currentEntity = -1;
        closestDistance = -1;
        foreach (int i in hoverFilter.Value)
            RayCast(ray, i, ref hoverFilter.Pools.Inc1.Get(i), ref hoverFilter.Pools.Inc2.Get(i));
        if (currentEntity >= 0)
            bus.Value.NewEventSingleton<InpEntityMouseHover>().entity = world.Value.PackEntity(currentEntity);
        else
            bus.Value.DestroyEventSingleton<InpEntityMouseHover>();
    }

    public void RayCast(Ray ray, int ent, ref ECPosition pos, ref ECMouseHoverable hover)
    {
        var point = pos.position2.Vec3() + hover.offset;
        float rayToClosestPoint = Vector3.Dot(point - ray.origin, ray.direction);
        var closestPoint = ray.GetPoint(rayToClosestPoint);
        float distToPoint = Vector3.Distance(point, closestPoint);
        if (distToPoint > hover.radius)
            return;
        //distance from ray origin to the surface of the sphere
        float rayToSurface = rayToClosestPoint - Mathf.Sqrt(hover.radius * hover.radius - distToPoint * distToPoint);
        if (closestDistance >= 0 && closestDistance < rayToSurface)
            return;
        currentEntity = ent;
        closestDistance = rayToSurface;
    }

}
public struct ECMouseHoverable
{
    public Vector3 offset;
    public float radius;
}

public struct InpEntityMouseHover : IEventSingleton
{
    public EcsPackedEntity entity;
}