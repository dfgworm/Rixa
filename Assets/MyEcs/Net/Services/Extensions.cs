using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

using Leopotam.EcsLite;
using MyEcs.Net;
using Mirror;

public static class ConnectionExtentions
{
    public static PlayerBehaviourBase GetPlayer(this NetworkConnection conn)
    {
        return PlayerBehaviourBase.GetByConnectionId(conn.connectionId);
    }

    public static bool OwnsNetId(this NetworkConnection conn, ushort netId)
    {
        var pb = conn.GetPlayer();
        return pb != null && pb.OwnsNetId(netId);
    }
}
