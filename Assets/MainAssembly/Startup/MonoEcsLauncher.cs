using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;

public class MonoEcsLauncher : MonoBehaviour
{
    EcsLauncher ecsSystems;
    private void Awake()
    {
        ecsSystems = new EcsLauncher();
        ecsSystems.Start();
    }
    private void Start()
    {
    }
    private void Update()
    {
        ecsSystems.Update();
    }
    private void FixedUpdate()
    {
        ecsSystems.FixedUpdate();
    }
    private void OnDestroy()
    {
        ecsSystems.Destroy();
    }
}
