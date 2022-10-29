using System.Collections;
using UnityEngine;
using Mirror;
using Leopotam.EcsLite;

using MyEcs.Spawn;
namespace MyEcs.Net
{
    abstract public class PlayerBehaviourBase : NetworkBehaviour
    {
        public static PlayerBehaviourBase localPlayer;
        public static PlayerBehaviourBase GetById(int playerId)
        {
            var pbs = FindObjectsOfType<PlayerBehaviourBase>();
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
        }
        abstract public void SpawnModel();
        [Server]
        public void DestroyModel()
        {
            if (!entity.Unpack(EcsStatic.world, out int ent))
                return;
            EcsStatic.GetPool<ECDestroy>().SoftAdd(ent);
        }
    }
    public struct ECPlayerBehaviour
    {
        public PlayerBehaviourBase pb;
    }
    public struct ECNetOwner
    {
        public int playerId;
        public bool IsClientAuthority => playerId > 0;
        public bool IsServerAuthority => playerId == 0;
        public bool BelongsToLocalPlayer => NetStatic.IsClient && playerId == PlayerBehaviourBase.localPlayer.playerId;
        public bool BelongsToOtherPlayer => NetStatic.IsClient && IsClientAuthority && playerId != PlayerBehaviourBase.localPlayer.playerId;
        public bool IsLocalAuthority => BelongsToLocalPlayer || (IsServerAuthority && NetStatic.IsServer);
        public static bool IsEntityLocalAuthority(int ent) => IsEntityLocalAuthority(EcsStatic.world, ent);
        public static bool IsEntityLocalAuthority(EcsWorld world, int ent)
        {
            if (world.GetPool<ECNetOwner>().Has(ent))
                return world.GetPool<ECNetOwner>().Get(ent).IsLocalAuthority;
            else
                return NetStatic.IsServer;
        }
    }
    public class NetOwnerBaggage : IBaggageAutoUnload
    {
        public int playerId;

        public void UnloadToWorld(EcsWorld world, int ent)
        {
            world.GetPool<ECNetOwner>().Add(ent).playerId = playerId;
        }
    }
}