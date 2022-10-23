using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class PlayerInputSystem : IEcsInitSystem, IEcsRunSystem
{


    readonly EcsCustomInject<EventBus> bus = default;

    Vector2 mousePos;
    Vector2 screenSize;
    Vector2 edgePercent = new Vector2(0.05f, 0.05f);

    float deadZone = 0.04f;

    public void Init(IEcsSystems systems)
    {
        mousePos = Input.mousePosition;
    }
    public void Run(IEcsSystems systems)
    {
        MouseMovementUpdate();
        screenSize = new Vector2(Screen.width, Screen.height);
        ScreenEdgeUpdate();
        MovementInputUpdate();
    }

    void MouseMovementUpdate()
    {
        Vector2 newPos = Input.mousePosition;
        var delta = newPos - mousePos;
        mousePos = newPos;
        if (delta != Vector2.zero)
            bus.Value.NewEventSingleton<EVMouseMoved>().dpos = delta;
        else
            bus.Value.DestroyEventSingleton<EVMouseMoved>();
    }
    void ScreenEdgeUpdate()
    {
        Vector2 relPos = mousePos / screenSize;
        Vector2 edge = Vector2.zero;

        if (Math.Abs(relPos.x) < edgePercent.x) edge.x = -1;
        else if (Math.Abs(1 - relPos.x) < edgePercent.x) edge.x = 1;
        if (Math.Abs(relPos.y) < edgePercent.y) edge.y = -1;
        else if (Math.Abs(1 - relPos.y) < edgePercent.y) edge.y = 1;

        if (edge != Vector2.zero)
            bus.Value.NewEventSingleton<EVScreenEdgeTouched>().edge = edge;
        else
            bus.Value.DestroyEventSingleton<EVScreenEdgeTouched>();
    }
    void MovementInputUpdate()
    {
        Vector2 dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Mathf.Abs(dir.x) <= deadZone)
            dir.x = 0;
        else
            dir.x = Mathf.Sign(dir.x);
        if (Mathf.Abs(dir.y) <= deadZone)
            dir.y = 0;
        else
            dir.y = Mathf.Sign(dir.y);

        if (dir != Vector2.zero)
            bus.Value.NewEventSingleton<EVMovementInput>().dpos = dir;
        else
            bus.Value.DestroyEventSingleton<EVMovementInput>();
    }
}

public struct EVScreenEdgeTouched : IEventSingleton
{
    public Vector2 edge;
}
public struct EVMouseMoved : IEventSingleton
{
    public Vector2 dpos;
}
public struct EVMovementInput : IEventSingleton
{
    public Vector2 dpos;
}