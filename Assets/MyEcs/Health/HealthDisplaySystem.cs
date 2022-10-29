using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MyEcs.Health
{
    public class HealthDisplaySystem : IEcsInitSystem, IEcsRunSystem
    {

        readonly EcsFilterInject<Inc<ECHealthDisplay, ECHealth>> healthFilter = default;
        readonly EcsFilterInject<Inc<ECHealthDisplay, ECPosition>> posFilter = default;


        public void Init(IEcsSystems systems)
        {

        }

        public void Run(IEcsSystems systems)
        {
            foreach (int i in posFilter.Value)
                UpdatePos(
                    ref posFilter.Pools.Inc1.Get(i),
                    ref posFilter.Pools.Inc2.Get(i)
                    );
            foreach (int i in healthFilter.Value)
                UpdateFill(
                    ref healthFilter.Pools.Inc1.Get(i),
                    ref healthFilter.Pools.Inc2.Get(i)
                    );
        }

        void UpdatePos(ref ECHealthDisplay disp, ref ECPosition pos)
        {
            disp.controller.MoveTo(new Vector3(pos.position.x, 0, pos.position.y));
        }
        void UpdateFill(ref ECHealthDisplay disp, ref ECHealth hp)
        {
            disp.controller.Fill = hp.Percent;
        }

    }
    public struct ECHealthDisplay : IEcsAutoReset<ECHealthDisplay>
    {
        public HealthDisplayController controller;
        static GameObject prefab;
        public void Init()
        {
            if (prefab == null)
                prefab = Resources.Load<GameObject>("Prefabs/HealthDisplay");
            controller = GameObject.Instantiate(prefab).GetComponent<HealthDisplayController>();
        }
        public void AutoReset(ref ECHealthDisplay c)
        {
            if (c.controller != null)
            {
                c.controller.Destroy();
                c.controller = null;
            }
        }
    }
}