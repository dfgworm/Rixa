using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

public class PlayerInputSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{

    readonly EcsCustomInject<EventBus> bus = default;

    Vector2 mousePos;
    Vector2 mouseWorldPos;
    Vector2 screenSize;
    Vector2 edgePercent = new Vector2(0.05f, 0.05f);

    float deadZone = 0.04f;
    ControlsManager controls;

    public void Init(IEcsSystems systems)
    {
        controls = new ControlsManager();
        controls.Enable();
        mousePos = Mouse.current.position.ReadValue();
    }
    public void Run(IEcsSystems systems)
    {
        MouseMovementUpdate();
        screenSize = new Vector2(Screen.width, Screen.height);
        ScreenEdgeUpdate();
        MovementInputUpdate();
        ActionInputUpdate();
    }
    public void Destroy(IEcsSystems systems)
    {
        controls.Disable();
        controls = null;
    }

    void MouseMovementUpdate()
    {
        Vector2 newPos = Mouse.current.position.ReadValue();
        var delta = newPos - mousePos;
        mousePos = newPos;
        if (Camera.main != null)
            CalculateMouseWorldPos();
        if (delta != Vector2.zero)
            bus.Value.NewEventSingleton<InpMouseMoved>().dpos = delta;
        else
            bus.Value.DestroyEventSingleton<InpMouseMoved>();
    }
    void CalculateMouseWorldPos()
    {
        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (ray.direction.y == 0)
            return;
        var vec = ray.direction / ray.direction.y * ray.origin.y;
        mouseWorldPos = (ray.origin - vec).Vec2();

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
            bus.Value.NewEventSingleton<InpScreenEdgeTouched>().edge = edge;
        else
            bus.Value.DestroyEventSingleton<InpScreenEdgeTouched>();
    }
    void MovementInputUpdate()
    {
        
        Vector2 dir = controls.Player.Move.ReadValue<Vector2>();
        if (Mathf.Abs(dir.x) <= deadZone)
            dir.x = 0;
        else
            dir.x = Mathf.Sign(dir.x);
        if (Mathf.Abs(dir.y) <= deadZone)
            dir.y = 0;
        else
            dir.y = Mathf.Sign(dir.y);

        if (dir != Vector2.zero)
            bus.Value.NewEventSingleton<InpMovementInput>().dpos = dir;
        else
            bus.Value.DestroyEventSingleton<InpMovementInput>();
    }
    void ActionInputUpdate()
    {
        if (controls.Player.Fire.triggered)
            bus.Value.NewEventSingleton<InpActionUse>().target = mouseWorldPos;
        else
            bus.Value.DestroyEventSingleton<InpActionUse>();
    }
}

public struct InpScreenEdgeTouched : IEventSingleton
{
    public Vector2 edge;
}
public struct InpMouseMoved : IEventSingleton
{
    public Vector2 dpos;
}
public struct InpMovementInput : IEventSingleton
{
    public Vector2 dpos;
}
public struct InpActionUse : IEventSingleton
{
    public Vector2 target;
}