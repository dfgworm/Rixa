using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;
using System.Linq;
using QuikGraph;
using JetBrains.Annotations;

namespace Generation
{
    public class ShipGenerator
    {
        public enum RoomType
        {
            empty,
            pilotCabin,
            engine,
            reactor,
            dock,
            hatch,
            livingRoom,
            storage
        }
        public LocationSchema GenerateSmallShip(int sizeX, int sizeY, int extraExitCount = 0)
        {
            var location = new LocationSchema(new Vector2Int(sizeX, sizeY));

            var cursor = location.GetCursor();

            //i might also want to randomly clip corners, filling them with 'empty'

            cursor.Reset()
                .MoveToX(sizeX/2)
                .MoveToEdge(direction.up)
                ;
            var pilotCabin = location.AddRoom(cursor.Cell.GetAllCells());
            pilotCabin.typeId = (int)RoomType.pilotCabin;

            cursor.Reset()
                .MoveToX(sizeX/2)
                .MoveToEdge(direction.down)
                ;
            var engine = location.AddRoom(cursor.Cell.GetAllCells());
            engine.typeId = (int)RoomType.engine;

            Cell cell;
            //at least 1 cell away from any edge
            cursor.Reset()
                .SelectAll()
                .Expand(direction.all, -1)
                .PickRandom(new Vector2Int(1,1), out cell)
                ;
            var reactor = location.AddRoom(cell);
            reactor.typeId = (int)RoomType.reactor;

            //left or right edge, except topmost and bottommost
            cursor.Reset()
                .MoveToEdge(MyRandom.RollPick(0.5f, direction.left, direction.right))
                .Expand(direction.vertical, sizeY)
                .Expand(direction.vertical, -1)
                .PickRandom(new Vector2Int(1,1), out cell)
                ;
            var dock = location.AddRoom(cell);
            dock.typeId = (int)RoomType.dock;

            var hatches = new List<RoomSchema>(extraExitCount);
            for (int i = 0; i < extraExitCount; i++)
            {
                //left or right edge, except topmost and bottommost
                bool succ = cursor.Reset()
                    .MoveToEdge(MyRandom.RollPick(0.5f, direction.left, direction.right))
                    .Expand(direction.vertical, sizeY)
                    .Expand(direction.vertical, -1)
                    .PickRandom(new Vector2Int(1, 1), out cell, true)
                    ;
                if (succ)
                {
                    var hatch = location.AddRoom(cell);
                    hatches.Add(hatch);
                    hatch.typeId = (int)RoomType.hatch;
                }
            }


            var otherRooms = location.FillEmptyWithRooms(1, 7); //livingRooms and storages
            foreach (var r in otherRooms)
            {
                if (MyRandom.Roll(0.5f))
                    r.typeId = (int)RoomType.livingRoom;
                else
                    r.typeId = (int)RoomType.storage;
            }



            location.BuildAdjacencyGraph();
            foreach(var room in location.adjacencySchema.adjacencyGraph.Vertices)
            {
                //some random connections between nodes
                var array = location.adjacencySchema.adjacencyGraph.AdjacentVertices(room).ToArray();
                int connections = array.Length;
                int exitCount = Mathf.CeilToInt(connections*Random.Range(0.2f, 0.8f) + Random.Range(-1, 1));
                for (int i = 0; i < exitCount; i++)
                {
                    var anotherRoom = array[Random.Range(0, connections)];
                    if (anotherRoom != location.emptiness)
                        location.adjacencySchema.AddPassage(room, anotherRoom);
                }

            }

            //location.adjacencySchema.EnsureAllRoomsConnected();


            return location;
        }
    }

}