using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Mirror;


public static class NetStatic
{
    public static bool IsServerBuild =>
#if UNITY_SERVER
            true;
#else
            false;
#endif
    public static bool IsServer =>
#if UNITY_SERVER
            true;
#else
            isServer;
#endif
    public static bool IsClient =>
#if UNITY_SERVER
            false;
#else
            isClient;
#endif
    static bool isServer = false;
    static bool isClient = false;
    public static void SetIsServer()
    {
        if (isClient)
            throw new Exception("SetIsClient has already been called");
        isServer = true;
    }
    public static void SetIsClient()
    {
        if (isServer)
            throw new Exception("SetIsServer has already been called");
        isClient = true;
    }
}
