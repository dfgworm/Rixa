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
    public class AdjacencySchema
    {
        public UndirectedGraph<RoomSchema, Adjacency> adjacencyGraph;
        //i want a separate passage graph so i can more easily traverse it without losing adjacency info
        public UndirectedGraph<RoomSchema, Passage> passageGraph;
        public AdjacencySchema()
        {
            adjacencyGraph = new UndirectedGraph<RoomSchema, Adjacency>(false);
            passageGraph = new UndirectedGraph<RoomSchema, Passage>(true);
        }
        public void AddRoom(RoomSchema room)
        {
            adjacencyGraph.AddVertex(room);
            passageGraph.AddVertex(room);
        }
        public void EnsureAllRoomsConnected()
        {
            throw new System.NotImplementedException();
        }
        public TaggedUndirectedEdge<RoomSchema, PassageData> AddPassage(RoomSchema room1, RoomSchema room2)
        {
            var edge = new Passage(room1, room2, new PassageData());
            passageGraph.AddEdge(edge);
            return edge;
        }
    }
    public class AdjacencyData
    {
        public int connections = 1;
    }
    public class PassageData
    {
    }
    public class Adjacency : TaggedUndirectedEdge<RoomSchema, AdjacencyData>
    {
        public Adjacency([NotNull] RoomSchema source, [NotNull] RoomSchema target, AdjacencyData tag)
            : base(source, target, tag) { }
        public Adjacency([NotNull] RoomSchema source, [NotNull] RoomSchema target)
            : this(source, target, new AdjacencyData()) { }

    }
    public class Passage : TaggedUndirectedEdge<RoomSchema, PassageData>
    {
        public Passage([NotNull] RoomSchema source, [NotNull] RoomSchema target, PassageData tag)
            : base(source, target, tag) { }
    }
}