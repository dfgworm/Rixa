using System.Collections;
using UnityEngine;
using Mirror;
using Leopotam.EcsLite;

using MyEcs.Spawn;
namespace MyEcs.Net
{

    public class PlayerBehaviour : PlayerBehaviourBase
    {
        [Server]
        override public void SpawnModel()
        {
            ref var ev = ref EcsStatic.bus.NewEvent<EVSpawn>();
            ev.Payload
                .Add(SpawnPipelineBaggage.Get<PlayerSP>())
                .Add(new PrefabIdBaggage { id = PrefabId.player })
                .Add(new PositionBaggage { position = new Vector2(2, 2) })
                .Add(new NetOwnerBaggage { playerId = playerId })
                .Add(NetIdBaggage.Allocate());
        }
    }
}