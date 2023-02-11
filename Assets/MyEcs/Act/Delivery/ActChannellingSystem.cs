using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace MyEcs.Act
{
    //i think i need to transmit channelling cancel, perhaps make it a separate action altogether? I will probably need it anyway
    public class ActChannellingSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<AMChannelingProcess> channelMarkPool = "act";
        readonly EcsPoolInject<ECPosition> posPool = default;
        readonly EcsPoolInject<ACChannelled> channelledPool = "act";

        readonly EcsFilterInject<Inc<AMChannelingProcess>> channelFilter = "act";
        readonly EcsFilterInject<Inc<ACChannelDisplay>> channelDisplayFilter = "act";

        readonly EcsFilterInject<Inc<AMUsed, ACChannelled>> useFilter = "act";

        public void Run(IEcsSystems systems)
        {
            foreach (int i in channelFilter.Value)
                UpdateChannel(i, ref channelFilter.Pools.Inc1.Get(i));
            foreach (int i in channelDisplayFilter.Value)
                UpdateChannelDisplay(i,ref channelDisplayFilter.Pools.Inc1.Get(i));

            foreach (int i in useFilter.Value)
                foreach (var usage in useFilter.Pools.Inc1.Get(i).usages)
                    ProcessActUse(i, usage);
        }

        void UpdateChannel(int act, ref AMChannelingProcess channel)
        {
            channel.timer += Time.deltaTime;
            if (channel.timer >= channel.duration)
                FinishChannel(act, channel);
        }
        void FinishChannel(int act, AMChannelingProcess channel)
        {
            channelMarkPool.Value.Del(act);
            if (!channelledPool.Value.Has(act) || !channelledPool.Value.Get(act).finishAct.Unpack(ActService.world, out int finishAct))
                return;

            ActService.GetPool<AMUsed>().SafeAdd(finishAct).usages.Add(channel.usage);
        }
        void UpdateChannelDisplay(int act, ref ACChannelDisplay display)
        {
            if (!channelMarkPool.Value.Has(act))
            {
                if (display.controller.gameObject.activeSelf)
                    display.controller.gameObject.SetActive(false);
                return;
            }
            if (!display.controller.gameObject.activeSelf)
                display.controller.gameObject.SetActive(true);
            int ent = ActService.GetEntity(act);
            if (posPool.Value.Has(ent))
                display.controller.MoveTo(posPool.Value.Get(ent).position3);
            var mark = channelMarkPool.Value.Get(act);
            display.controller.fill = mark.timer / mark.duration;
        }
        void ProcessActUse(int ac, ActUsageContainer usage)
        {
            if (channelMarkPool.Value.Has(ac))
                return;

            ref var acChannel = ref channelledPool.Value.Get(ac);
            ref var mark = ref channelMarkPool.Value.Add(ac);
            mark.usage = usage;
            mark.duration = acChannel.duration;
        }
    }
    public struct ACChannelled
    {
        public float duration;
        public EcsPackedEntity finishAct;
    }
    public struct AMChannelingProcess
    {
        public float timer;
        public float duration;
        public ActUsageContainer usage;
    }
    public struct ACChannelDisplay : IEcsAutoReset<ACChannelDisplay>
    {
        public FillBarController controller;
        static GameObject prefab;
        public void Init()
        {
            if (prefab == null)
                prefab = Resources.Load<GameObject>("Prefabs/ChannelDisplay");
            controller = GameObject.Instantiate(prefab).GetComponent<FillBarController>();
        }
        public void AutoReset(ref ACChannelDisplay c)
        {
            if (c.controller != null)
            {
                c.controller.Destroy();
                c.controller = null;
            }
        }
    }
}