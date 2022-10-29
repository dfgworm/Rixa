using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

namespace MyEcs.Health
{
    public class HealthDisplayController : MonoBehaviour
    {
        public Vector3 shift;
        public Image filler;
        public float Fill { get; set; }
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
            filler.fillAmount = Fill;
            if (Camera.main != null)
                gameObject.transform.LookAt(Camera.main.transform.position);
        }
    }
}