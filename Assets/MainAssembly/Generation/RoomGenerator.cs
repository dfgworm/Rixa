using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;
using System;

namespace Generation
{
    public class RoomGenerator
    {
        Vector2Int enemyCount;
        public RoomSchema Generate()
        {
            var room = new RoomSchema();
            int enemyC = UnityEngine.Random.Range(enemyCount.x, enemyCount.y);
            for (int i = 0; i < enemyC; i++)
                room.AddEnemy();

            return room;
        }
        public RoomGenerator SetEnemyCount(int min, int max = -1)
        {
            if (max < 0)
                max = min;
            enemyCount = new Vector2Int(min, max);
            return this;
        }
    }

    public class RoomSchema : IComparable
    {
        static int id;
        public int roomId;
        public int typeId;
        public HashSet<Cell> cells;
        public RoomSchema()
        {
            cells = new HashSet<Cell>();
            roomId = id;
            id++;
        }
        public void AddCell(Cell cell)
        {
            foreach (Cell c in cell.GetAllCells())
                cells.Add(c); //add cells of size 1
        }
        public RoomSchema AddEnemy()
        {

            return this;
        }
        public RoomSchema AddExit()
        {

            return this;
        }

        public int CompareTo(object obj)
            => 0;
    }
}