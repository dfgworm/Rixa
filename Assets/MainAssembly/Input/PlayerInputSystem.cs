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

    const float DEADZONE = 0.04f;
    static public ControlsManager controls;

    static public void ConnectActToInput(InputAction inp, int ac)
    {
        ref var connected = ref MyEcs.Acts.ActService.GetPool<ACConnectedInputAct>().SoftAdd(ac);
        connected.input = inp;
        EcsPackedEntity packedAc = MyEcs.Acts.ActService.world.PackEntity(ac);
        connected.Connect(delegate (InputAction.CallbackContext context)
        {
            ref var ev = ref EcsStatic.bus.NewEvent<InpActUse>();
            ev.act = packedAc;
            ev.context = context;
        });

    }
    public void Init(IEcsSystems systems)
    {
        controls = new ControlsManager();
        controls.Enable();
        
        mousePos = Mouse.current.position.ReadValue();
        MouseUpdate();
    }
    public void Run(IEcsSystems systems)
    {
        MouseUpdate();
        screenSize = new Vector2(Screen.width, Screen.height);
        ScreenEdgeUpdate();
        MovementInputUpdate();
    }
    public void Destroy(IEcsSystems systems)
    {
        controls.Disable();
        controls = null;
    }

    void MouseUpdate()
    {
        Vector2 newPos = Mouse.current.position.ReadValue();
        var delta = newPos - mousePos;
        mousePos = newPos;
        if (Camera.main != null)
            CalculateMouseWorldPos();
        bus.Value.NewEventSingleton<InpMousePosition>().pos = mousePos;
        bus.Value.NewEventSingleton<InpMouseWorldPosition>().pos = mouseWorldPos;
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
        if (Mathf.Abs(dir.x) <= DEADZONE)
            dir.x = 0;
        else
            dir.x = Mathf.Sign(dir.x);
        if (Mathf.Abs(dir.y) <= DEADZONE)
            dir.y = 0;
        else
            dir.y = Mathf.Sign(dir.y);

        if (dir != Vector2.zero)
            bus.Value.NewEventSingleton<InpMovement>().dpos = dir;
        else
            bus.Value.DestroyEventSingleton<InpMovement>();
    }
}

public struct InpScreenEdgeTouched : IEventSingleton
{
    public Vector2 edge;
}
public struct InpMousePosition : IEventSingleton
{
    public Vector2 pos;
}
public struct InpMouseWorldPosition : IEventSingleton
{
    public Vector2 pos;
}
public struct InpMouseMoved : IEventSingleton
{
    public Vector2 dpos;
}
public struct InpMovement : IEventSingleton
{
    public Vector2 dpos;
}
public struct InpActUse : IEventReplicant
{
    public InputAction.CallbackContext context;
    public EcsPackedEntity act;
}
public struct ACConnectedInputAct : IEcsAutoReset<ACConnectedInputAct>
{
    public InputAction input;
    Action<InputAction.CallbackContext> connectedFunction;

    public void Connect(Action<InputAction.CallbackContext> action)
    {
        connectedFunction = action;
        input.performed += connectedFunction;
    }
    public void AutoReset(ref ACConnectedInputAct c)
    {
        if (c.input != null && c.connectedFunction != null)
            c.input.performed -= c.connectedFunction;
    }
}
