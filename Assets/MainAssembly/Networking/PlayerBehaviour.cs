using System.Collections;
using UnityEngine;
using Mirror;
using Leopotam.EcsLite;

using MyEcs.Spawn;
using MyEcs.Net;

public class PlayerBehaviour : NetworkBehaviour
{
    public static PlayerBehaviour localPlayer;
    public static PlayerBehaviour GetById(int playerId)
    {
        var pbs = FindObjectsOfType<PlayerBehaviour>();
        foreach (var pb in pbs)
            if (pb.playerId == playerId)
                return pb;
        return null;
    }
    [SyncVar]
    public int playerId;
    public NetworkConnectionToClient connection;
    public EcsPackedEntity entity;
    public void Awake()
    {
            
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    public override void OnStartAuthority()
    {
        localPlayer = this;
        CmdReady();
        Debug.Log("PB Authority started");
    }
    public override void OnStopAuthority()
    {
        localPlayer = null;
    }
    [Command]
    public void CmdReady()
    {
        NetSpawnSystem.SendAllToClient(connection);
        SpawnModel();
        Debug.Log("CmdReady");
    }
    [Server]
    public void SpawnModel()
    {
        ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
        ev.payload = new BaggagePayload()
            .Add(new SpawnPipelineBaggage(typeof(PlayerSpawnPipeline)))
            .Add(new PrefabIdBaggage { id = PrefabId.player } )
            .Add(new PositionBaggage { position = new Vector2(2,2) } )
            .Add(new NetOwnerBaggage { playerId = playerId })
            .Add(new NetIdBaggage(true));
    }
    [Server]
    public void DestroyModel()
    {
        if (!entity.Unpack(EcsStatic.world, out int ent))
            return;
        EcsStatic.GetPool<ECDestroy>().SoftAdd(ent);
    }
}
public struct ECLocalPlayer
{
}
public struct ECPlayerBehaviour
{
    public PlayerBehaviour pb;
}
public struct ECNetOwner
{
    public int playerId;
    public bool BelongsToPlayer => playerId > 0;
    public bool BelongsToServer => playerId == 0;
    public bool BelongsToLocalPlayer => NetStatic.IsClient && playerId == PlayerBehaviour.localPlayer.playerId;
    public bool BelongsToOtherPlayer => NetStatic.IsClient && BelongsToPlayer && playerId != PlayerBehaviour.localPlayer.playerId;
    public bool HasActiveOwnership => BelongsToLocalPlayer || (BelongsToServer && NetStatic.IsServer);
}
public class NetOwnerBaggage : IBaggage
{
    public int playerId;

    public void UnloadToWorld(EcsWorld world, int ent)
    {
        world.GetPool<ECNetOwner>().Add(ent).playerId = playerId;
    }
}
