using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Mirror;

namespace MyEcs.Net
{
    public static class NetCommunication
    {
        [Server]
        public static void SendToClient<T>(NetworkConnectionToClient conn, T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {
            conn.Send(msg, channel);
        }
        [Server]
        public static void Broadcast<T>(T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {

            NetworkServer.SendToAll(msg, channel);
        }
        [Server]
        public static void BroadcastExcept<T>(int exceptionId, T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                if (exceptionId != conn.connectionId)
                    conn.Send(msg, channel);
        }
        [Server]
        public static void BroadcastToReady<T>(T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {
            NetworkServer.SendToReady(msg, channel);
        }
        [Server]
        public static void BroadcastToReadyExcept<T>(int exceptionId, T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                if (exceptionId != conn.connectionId && conn.isReady)
                    conn.Send(msg, channel);
        }
        [Client]
        public static void SendToServer<T>(T msg, int channel = Channels.Unreliable)
            where T : struct, NetworkMessage
        {
            NetworkClient.Send(msg, channel);
        }
    }
}