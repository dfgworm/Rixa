using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using MyEcs.Net;

namespace MyEcs.Actions
{
    //i think i need to transmit channelling cancel, perhaps make it a separate action altogether? I will probably need it anyway
    //i also really need to check dublicates of the same action the might be generated because of this weird selective net transmission
    //i might want to solidify the system, non-local entities should not be simulated at all, just transmit everything through net
    public class ChannellingSystem : IEcsRunSystem
    {
        readonly EcsPoolInject<EMChannelling> channelMarkPool = default;
        readonly EcsPoolInject<ECPosition> posPool = default;
        readonly EcsPoolInject<ACChannelled> channelledPool = "actions";

        //readonly EcsWorldInject world = default;
        readonly EcsWorldInject acWorld = "actions";

        readonly EcsCustomInject<EventBus> bus = default;

        readonly EcsFilterInject<Inc<EMChannelling>> channelFilter = default;
        readonly EcsFilterInject<Inc<ECChannelDisplay>> channelDisplayFilter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (int i in channelFilter.Value)
                UpdateChannel(i, ref channelFilter.Pools.Inc1.Get(i));
            foreach (int i in channelDisplayFilter.Value)
                UpdateChannelDisplay(i,ref channelDisplayFilter.Pools.Inc1.Get(i));
            foreach (int i in bus.Value.GetEventBodies<AEVUse>(out var pool))
                ProcessActionUse(ref pool.Get(i));
        }

        void UpdateChannel(int ent, ref EMChannelling channel)
        {
            channel.timer += Time.deltaTime;
            if (channel.timer >= channel.duration)
                FinishChannel(ent, channel);
        }
        void FinishChannel(int ent, EMChannelling channel)
        {
            channelMarkPool.Value.Del(ent);
            if (!NetOwnershipService.IsEntityLocalAuthority(ent))
                return;
            if (!channel.useEvent.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!channelledPool.Value.Has(ac))
                return;


            ref var useEv = ref bus.Value.NewEvent<AEVUse>();
            useEv = channel.useEvent;
            useEv.action = channelledPool.Value.Get(ac).finishAction;
        }
        void UpdateChannelDisplay(int ent, ref ECChannelDisplay display)
        {
            if (!channelMarkPool.Value.Has(ent))
            {
                if (display.controller.gameObject.activeSelf)
                    display.controller.gameObject.SetActive(false);
                return;
            }
            if (!display.controller.gameObject.activeSelf)
                display.controller.gameObject.SetActive(true);
            if (posPool.Value.Has(ent))
                display.controller.MoveTo(posPool.Value.Get(ent).position3);
            var mark = channelMarkPool.Value.Get(ent);
            display.controller.fill = mark.timer / mark.duration;
        }
        void ProcessActionUse(ref AEVUse ev)
        {
            if (!ev.action.Unpack(acWorld.Value, out int ac))
                return;
            if (!channelledPool.Value.Has(ac))
                return;
            if (!EcsActionService.TryGetEntity(ac, out int ent))
                return;
            if (channelMarkPool.Value.Has(ent))
                return;

            ref var acChannel = ref channelledPool.Value.Get(ac);
            ref var mark = ref channelMarkPool.Value.Add(ent);
            mark.useEvent = ev;
            mark.duration = acChannel.duration;
        }
    }
    public struct ACChannelled
    {
        public float duration;
        public EcsPackedEntity finishAction;
    }
    public struct EMChannelling
    {
        public float timer;
        public float duration;
        public AEVUse useEvent;
    }
    public struct ECChannelDisplay : IEcsAutoReset<ECChannelDisplay>
    {
        public FillBarController controller;
        static GameObject prefab;
        public void Init()
        {
            if (prefab == null)
                prefab = Resources.Load<GameObject>("Prefabs/ChannelDisplay");
            controller = GameObject.Instantiate(prefab).GetComponent<FillBarController>();
        }
        public void AutoReset(ref ECChannelDisplay c)
        {
            if (c.controller != null)
            {
                c.controller.Destroy();
                c.controller = null;
            }
        }
    }
}