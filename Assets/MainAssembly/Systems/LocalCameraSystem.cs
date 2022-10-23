using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Physics;

public class LocalCameraSystem : IEcsRunSystem
{
    public float edgeTouchSpeed = 10;
    public float zoomSensivity = 1;
    public float dragSensivity = 30;

    readonly EcsFilterInject<Inc<ECCameraFocus>> focusFilter = default;

    readonly EcsCustomInject<EventBus> bus = default;

    public void Run(IEcsSystems systems)
    {
        //if (NetStatic.IsServer)
        //    return;
        foreach (int i in focusFilter.Value)
            UpdateCamera(focusFilter.Pools.Inc1.Get(i).focus);
    }

    void UpdateCamera(CameraFocus focus)
    {
        if (Input.GetMouseButton(2))
            ObeyMouseMovement(focus);
        else
            ObeyEdgeTouch(focus);
        UpdateZoomScroll(focus);
    }

    void ObeyMouseMovement(CameraFocus focus)
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        if (bus.Value.HasEventSingleton<EVMouseMoved>())
            focus.Position -= bus.Value.GetEventBodySingleton<EVMouseMoved>().dpos / screenSize * dragSensivity;
    }

    void ObeyEdgeTouch(CameraFocus focus)
    {
        if (bus.Value.HasEventSingleton<EVScreenEdgeTouched>())
            focus.Position += bus.Value.GetEventBodySingleton<EVScreenEdgeTouched>().edge * Time.deltaTime * edgeTouchSpeed;
    }
    void UpdateZoomScroll(CameraFocus focus)
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
            focus.SetCameraDistance(focus.camDistance - scroll * zoomSensivity);
    }
}

public struct ECCameraFocus
{
    public CameraFocus focus;
}