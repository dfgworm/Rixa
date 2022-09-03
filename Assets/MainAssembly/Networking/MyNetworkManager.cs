using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Mirror;

using MyEcs.Net;

public class MyNetworkManager : NetworkManager
{
    override public void Awake()
    {
        base.Awake(); //initializes singleton, changes networkSceneName and setups OnSceneLoaded callback
        BaggageSerializer.Init();
    }
    public override void Start()
    {
        // headless mode? then start the server
        // can't do this in Awake because Awake is for initialization.
        // some transports might not be ready until Start.
        //
        // (tick rate is applied in StartServer!)
#if UNITY_SERVER
            if (autoStartServerBuild)
            {
                StartServer();
            }
#endif
        DontDestroyOnLoad(this);
    }
    public override void OnStartServer()
    {
        NetStatic.SetIsServer();
        Debug.Log("Server Started");
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("Player Connected");
    }
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        if (conn.identity == null)
        {
            // this is now allowed (was not for a while)
            //Debug.Log("Ready with no player object");
        }
        Debug.Log("Client ready");
        NetworkServer.SetClientReady(conn);
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);

        PlayerBehaviour pb = player.GetComponent<PlayerBehaviour>();
        var data = (AuthData)conn.authenticationData;
        pb.playerId = data.playerId;
        pb.connection = conn;
        player.name = $"PlayerBehaviour{pb.playerId} [connId={conn.connectionId}]";

        NetworkServer.AddPlayerForConnection(conn, player);
    }
    public override void OnStartClient()
    {
        NetStatic.SetIsClient();
        Debug.Log("Client Started");
    }
    public override void OnClientConnect()
    {
        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load completing.
            // if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!NetworkClient.ready)
                NetworkClient.Ready();

            if (autoCreatePlayer)
                NetworkClient.AddPlayer();
        }
        Debug.Log("Connected to server");
    }
    public override void OnClientSceneChanged()
    {
        // always become ready.
        if (!NetworkClient.ready) NetworkClient.Ready();

        // Only call AddPlayer for normal scene changes, not additive load/unload
        //if (clientSceneOperation == SceneOperation.Normal && autoCreatePlayer && NetworkClient.localPlayer == null)
        if (autoCreatePlayer && NetworkClient.localPlayer == null)
        {
            // add player if existing one is null
            NetworkClient.AddPlayer();
        }
    }

    public override void OnClientDisconnect()
    {
        if (mode == NetworkManagerMode.Offline)
            return;

        StopClient();
        Debug.Log("Disconnecting from server");
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        var pb = conn.identity?.GetComponent<PlayerBehaviour>();
        pb?.DestroyModel();
        NetworkServer.DestroyPlayerForConnection(conn);
        Debug.Log("Player Disconnected");
    }

}