using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using System;


public class CameraFocus : MonoBehaviour
{
    public float camDistance = 40;
    public float camAngle = 60;
    public Vector2 Position
    {
        get => position;
        set
        {
            position = value;
            transform.position = new Vector3(value.x, 0, value.y);
        }
    }

    Camera cam;
    Vector2 position;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        UpdateCameraTransform();
        position = transform.position.Vec2();
    }
    public void SetCameraAngle(float angle)
    {
        camAngle = angle;
        UpdateCameraTransform();
    }

    public void SetCameraDistance(float distance)
    {
        camDistance = distance;
        UpdateCameraTransform();
    }
    void UpdateCameraTransform()
    {
        var camTr = cam.gameObject.transform;
        camTr.localEulerAngles = new Vector3(camAngle, 0, 0);
        camTr.localPosition = -camTr.forward * camDistance;
    }
}