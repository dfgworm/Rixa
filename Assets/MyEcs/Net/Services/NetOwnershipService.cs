using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Mirror;

namespace MyEcs.Net
{
    public static class NetOwnershipService
    {
        public static bool IsEntityLocalAuthority(int ent)
        {
            var world = EcsStatic.world;
            if (world.GetPool<ECNetOwner>().Has(ent))
                return world.GetPool<ECNetOwner>().Get(ent).IsLocalAuthority;
            else
                return NetStatic.IsServer;
        }
    }
}