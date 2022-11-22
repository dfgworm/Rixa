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
    public class LocationSchema : ICellSelectable
    {
        public Vector2Int Size { get; private set; }
        public Cell Cell { get => new Cell { size = Size }; }
        public AdjacencySchema adjacencySchema;
        public RoomSchema emptiness;
        RoomSchema[][] grid;
        public LocationSchema(Vector2Int _size)
        {
            Size = _size;
            grid = new RoomSchema[Size.y][];
            for (int i = 0; i < Size.y; i++)
                grid[i] = new RoomSchema[Size.x];
            adjacencySchema = new AdjacencySchema();
            emptiness = new RoomSchema();
            adjacencySchema.AddRoom(emptiness);
        }
        public CellCursor GetCursor()
        {
            return new CellCursor(this);
        }
        public RoomSchema AddRoom(Cell c, RoomSchema room = null)
            => AddRoom(c.GetAllCells(), room);
        public RoomSchema AddRoom(IEnumerable<Cell> cells, RoomSchema room = null)
        {
            if (room == null)
                room = new RoomSchema();
            foreach (Cell c in cells)
            {
                int x = c.pos.x;
                int y = c.pos.y;
                if (grid[y][x] != null)
                    throw new System.Exception($"Room already added at ({x}, {y})");
                grid[y][x] = room;
                room.AddCell(c);
            }
            adjacencySchema.AddRoom(room);
            return room;
        }
        public void BuildAdjacencyGraph()
        {
            foreach(Cell c in Cell.GetAllCells())
            {
                if (c.pos.x != 0) //except leftmost
                    Process(c, new Cell { size = c.size, pos = c.pos + Vector2Int.left });
                if (c.pos.y != 0) //except bottommost
                    Process(c, new Cell { size = c.size, pos = c.pos + Vector2Int.down });
                if (c.pos.x == 0 || c.pos.y == 0 //for edge cells there must be a connection to emptiness
                    || c.pos.x == Size.x - 1 || c.pos.y == Size.y - 1)
                    AddConnection(GetRoomAt(c), emptiness);
            }
            void Process(Cell c1, Cell c2)
            {
                AddConnection(GetRoomAt(c1), GetRoomAt(c2));
            }
            void AddConnection(RoomSchema room1, RoomSchema room2)
            {
                if (room1 == room2)
                    return;
                Adjacency edge;
                if (adjacencySchema.adjacencyGraph.TryGetEdge(room1, room2, out edge))
                    edge.Tag.connections += 1;
                else
                    adjacencySchema.adjacencyGraph.AddEdge(new Adjacency(room1 ,room2));
            }
        }
        public HashSet<RoomSchema> FillEmptyWithRooms(int minSpread = 0, int maxSpread = 1)
        {
            int maxRetryCount = 5;
            var list = new HashSet<RoomSchema>();

            void TryMakeRoom(Cell cell)
            {
                if (IsOccupied(cell))
                    return;
                var cells = new HashSet<Cell>();

                int targetSize = Random.Range(minSpread, maxSpread);

                int added = 0;
                cells.Add(cell);
                bool TrySpread()
                {
                    Cell c = MyRandom.Pick(MyRandom.Pick(cells).GetNeighbours());
                    if (!IsWithin(c) || IsOccupied(c))
                        return false;
                    cells.Add(c);
                    added++;
                    return true;
                }
                int retryCounter = 0;
                while (added < targetSize)
                {
                    bool res = TrySpread();
                    if (res)
                        retryCounter = 0;
                    else
                    {
                        if (retryCounter > maxRetryCount)
                            break;
                        retryCounter++;
                    }

                }

                var room = new RoomSchema();
                AddRoom(cells, room);
                list.Add(room);
            }
            var array = GetCursor().SelectAll().GetAllUnoccupiedCells().ToArray();
            MyRandom.Shuffle(array);
            foreach (Cell cell in array)
                TryMakeRoom(cell);
            return list;
        }
        public bool IsOccupied(Cell cell)
        {
            var bounds = cell.pos + cell.size;
            for (int y = cell.pos.y; y < bounds.y; y++)
                for (int x = cell.pos.x; x < bounds.x; x++)
                    if (IsOccupied(x,y))
                        return true;
            return false;
        }
        public bool IsOccupied(int x, int y)
            => grid[y][x] != null;
        public bool IsWithin(Cell cell)
        {
            var bounds = cell.pos + cell.size;
            for (int y = cell.pos.y; y < bounds.y; y++)
                for (int x = cell.pos.x; x < bounds.x; x++)
                    if (!IsWithin(x, y))
                        return false;
            return true;
        }
        public bool IsWithin(int x, int y)
            => x >= 0 && x < Size.x && y >= 0 && y < Size.y;
        public RoomSchema GetRoomAt(Cell c)
            => GetRoomAt(c.pos.x, c.pos.y);

        public RoomSchema GetRoomAt(int x, int y)
        {
            var r = grid[y][x];
            if (r == null)
                return emptiness;
            else
                return r;
        }
    }
}