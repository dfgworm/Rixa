using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

public class FillBarController : MonoBehaviour
{
    public Vector3 shift;
    public Image filler;
    public float fill { get; set; }
    public void MoveTo(Vector3 pos)
    {
        transform.position = pos + shift;
    }
    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
    private void Update()
    {
        filler.fillAmount = fill;
        if (Camera.main != null)
            gameObject.transform.LookAt(Camera.main.transform.position);
    }
}