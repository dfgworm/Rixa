using System;
using UnityEngine;

using Mirror;

public class MyAuthenticator : NetworkAuthenticator
{
    #region Messages

    public struct AuthRequestMessage : NetworkMessage
    {
        public string clientDeviceID;
    }

    public struct AuthResponseMessage : NetworkMessage { }

    #endregion

    #region Server

    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnStopServer()
    {
        // unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // do nothing, wait for client to send his id
    }

    int nextPlayerId = 1;
    void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        var data = new AuthData { deviceId = msg.clientDeviceID, playerId = nextPlayerId };
        nextPlayerId++;
        conn.authenticationData = data;

        Debug.Log($"connection {conn.connectionId} player {data.playerId} authenticated with id {msg.clientDeviceID}");

        conn.Send(new AuthResponseMessage());

        ServerAccept(conn);
    }


    #endregion

    #region Client

    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    public override void OnStopClient()
    {
        // unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    public override void OnClientAuthenticate()
    {
        string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;

        // Not all platforms support this, so we use a GUID instead
        if (deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier)
        {
            // Get the value from PlayerPrefs if it exists, new GUID if it doesn't
            deviceUniqueIdentifier = PlayerPrefs.GetString("deviceUniqueIdentifier", Guid.NewGuid().ToString());

            // Store the deviceUniqueIdentifier to PlayerPrefs (in case we just made a new GUID)
            PlayerPrefs.SetString("deviceUniqueIdentifier", deviceUniqueIdentifier);
        }

        // send the deviceUniqueIdentifier to the server
        NetworkClient.connection.Send(new AuthRequestMessage { clientDeviceID = deviceUniqueIdentifier } );
    }

    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        Debug.Log("Authentication Success");
        ClientAccept();
    }

    #endregion
}

public class AuthData
{
    public string deviceId;
    public int playerId;
}