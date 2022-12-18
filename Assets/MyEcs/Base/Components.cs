using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;


public struct ECGameObject
{
    public GameObject gameObject;
}
public struct ECPosition
{
    public Vector2 position2;
    public Vector3 position3 {
        get => new Vector3(position2.x, 0, position2.y);
        set => position2.Set(value.x, value.z);
    }
}
public struct ECRotation
{
    public float rotation;
}
