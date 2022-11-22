using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Leopotam.EcsLite;
using System.Linq;

[System.Flags]
public enum direction
{
    other = 0,
    up = 1,
    down = 2,
    left = 4,
    right = 8,
    horizontal = left | right,
    vertical = up | down,
    all = up | down | left | right,
}

namespace Generation
{
    public interface ICellSelectable
    {
        Vector2Int Size { get; }
        bool IsOccupied(Cell cell);
    }
    public struct Cell
    {
        public Vector2Int pos;
        public Vector2Int size;

        public Cell(Vector2Int _pos)
        {
            pos = _pos;
            size = Vector2Int.one;
        }
        public Cell(Vector2Int _pos, Vector2Int _size)
        {
            pos = _pos;
            size = _size;
        }
        public Cell(int x, int y, int sX = 1, int sY = 1)
        {
            pos = new Vector2Int(x, y);
            size = new Vector2Int(sX, sY);
        }

        public static bool operator ==(Cell a, Cell b)
            => a.pos == b.pos && a.size == b.size;
        public static bool operator !=(Cell a, Cell b)
            => !(a == b);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Cell other = (Cell)obj;
            return other == this;
        }
        public override int GetHashCode()
         => pos.GetHashCode() ^ (size.GetHashCode() << 16);
        public override string ToString()
            => pos.ToString()+":"+size.ToString();
        public HashSet<Cell> GetNeighbours() //works as if this is a (1,1) size cell
        {
            var list = new HashSet<Cell>();
            void validateAdd(Cell c)
            {
                if (c.pos.x < 0 || c.pos.y < 0)
                    return;
                list.Add(c);
            }
            validateAdd(new Cell(pos + Vector2Int.up));
            validateAdd(new Cell(pos + Vector2Int.down));
            validateAdd(new Cell(pos + Vector2Int.left));
            validateAdd(new Cell(pos + Vector2Int.right));
            return list;
        }
        public HashSet<Cell> GetAllCells()
            => GetAllCells(Vector2Int.one);
        public HashSet<Cell> GetAllCells(Vector2Int splitSize) //returns all possible cells of size splitSize
        {
            var s = size - splitSize + Vector2Int.one;
            var list = new HashSet<Cell>(s.x * s.y);
            for (int y = pos.y; y < pos.y + s.y; y++)
                for (int x = pos.x; x < pos.x + s.x; x++)
                    list.Add(new Cell { size = splitSize, pos = new Vector2Int(x, y) });
            return list;
        }
    }
    public class CellCursor
    {
        public CellCursor(ICellSelectable _sel)
        {
            selectable = _sel;
        }
        public int posX = 0;
        public int posY = 0;
        public int sizeX = 1;
        public int sizeY = 1;
        ICellSelectable selectable;
        public Cell Cell {
            get => new Cell(posX, posY, sizeX, sizeY);
            set {
                posX = value.pos.x;
                posY = value.pos.y;
                sizeX = value.size.x;
                sizeY = value.size.y;
                Trim();
            }
        }
        public HashSet<Cell> GetAllUnoccupiedCells()
            => GetAllUnoccupiedCells(Vector2Int.one);
        public HashSet<Cell> GetAllUnoccupiedCells(Vector2Int size)
        {
            return Cell.GetAllCells(size).Where((c) => !IsOccupied(c)).ToHashSet();
        }
        public HashSet<Cell> FillGetCells()
        {
            var list = new HashSet<Cell>();
            void recursiveAdd(Cell cell)
            {
                if (IsOccupied(cell) || list.Contains(cell) ||
                    cell.pos.x < 0 || cell.pos.y < 0 ||
                    cell.pos.x >= selectable.Size.x || cell.pos.y >= selectable.Size.y)
                    return;
                list.Add(cell);
                foreach (Cell c in cell.GetNeighbours())
                    recursiveAdd(c);
            }
            recursiveAdd(Cell);
            return list;
        }
        public bool PickRandom(Vector2Int size, out Cell cell, bool unoccupied = true)
        {
            var list = unoccupied ? GetAllUnoccupiedCells(size) : Cell.GetAllCells(size);
            int c = list.Count;
            if (c == 0) {
                cell = Cell;
                return false;
            } else {
                cell = list.ElementAt(Random.Range(0, c));
                return true;
            }
        }
        public CellCursor SelectAll(direction dir = direction.all)
        {
            if (dir.HasFlag(direction.vertical))
                Expand(direction.vertical, selectable.Size.y);
            if (dir.HasFlag(direction.horizontal))
                Expand(direction.horizontal, selectable.Size.x);
            return this;
        }
        public CellCursor MoveToEdge(direction dir)
        {
            if (dir.HasFlag(direction.down))
                posY = 0;
            else if (dir.HasFlag(direction.up))
                posY = selectable.Size.y - sizeY;
            if (dir.HasFlag(direction.left))
                posX = 0;
            else if (dir.HasFlag(direction.right))
                posX = selectable.Size.x - sizeX;
            return this;
        }
        public CellCursor MoveCenterTo(int x, int y)
        {
            //-1 here is needed so that left-bottom corner of the selector is considered center
            posX = Mathf.Clamp(x-Mathf.FloorToInt((sizeX-1) /2), 0, selectable.Size.x-1);
            posY = Mathf.Clamp(y-Mathf.FloorToInt((sizeY-1) /2), 0, selectable.Size.y-1);
            Trim();
            return this;
        }
        public CellCursor MoveTo(int x, int y)
        {
            posX = Mathf.Clamp(x, 0, selectable.Size.x - 1);
            posY = Mathf.Clamp(y, 0, selectable.Size.y - 1);
            Trim();
            return this;
        }
        public CellCursor SetCell(Cell c)
        {
            Cell = c;
            return this;
        }
        public CellCursor MoveToX(int pos) => MoveTo(pos, posY);
        public CellCursor MoveToY(int pos) => MoveTo(posX, pos);
        public CellCursor Move(direction dir, int amount)
        {
            if (dir.HasFlag(direction.up))
                posY += amount;
            else if (dir.HasFlag(direction.down))
                posY -= amount;
            if (dir.HasFlag(direction.right))
                posX += amount;
            else if (dir.HasFlag(direction.left))
                posX -= amount;
            Trim();
            return this;
        }
        public CellCursor Expand(direction dir, int amount)
        {
            int amY = amount;
            if (amY < 0) {
                if (dir.HasFlag(direction.vertical))
                    amY = Mathf.Max(amY, -((sizeY-1) /2));
                else if (dir.HasFlag(direction.down))
                    amY = Mathf.Max(amY, -sizeY + 1);
            }
            int amX = amount;
            if (amX < 0)
            {
                if (dir.HasFlag(direction.horizontal))
                    amX = Mathf.Max(amX, -((sizeX-1) /2));
                else if (dir.HasFlag(direction.left))
                    amX = Mathf.Max(amX, -sizeX + 1);
            }

            if (dir.HasFlag(direction.up))
                sizeY += amY;
            if (dir.HasFlag(direction.down))
            {
                sizeY += amY;
                posY -= amY;
            }
            if (dir.HasFlag(direction.right))
                sizeX += amX;
            if (dir.HasFlag(direction.left))
            {
                sizeX += amX;
                posX -= amX;
            }
            Trim();
            return this;
        }
        public bool IsOccupied(Cell c)
            => (selectable != null) && selectable.IsOccupied(c);
        public CellCursor Reset()
        {
            Cell = new Cell { size = Vector2Int.one, pos = Vector2Int.zero };
            return this;
        }
        void Trim() //Remove from selection all cells that are out of location bounds
        {
            var maxSize = selectable.Size;
            if (posX < 0)
                sizeX += posX;
            if (posY < 0)
                sizeY += posY;
            posX = Mathf.Clamp(posX, 0, maxSize.x-1);
            posY = Mathf.Clamp(posY, 0, maxSize.y-1);
            int excessSize = posX + sizeX - maxSize.x;
            if (excessSize > 0)
                sizeX -= excessSize;
            excessSize = posY + sizeY - maxSize.y;
            if (excessSize > 0)
                sizeY -= excessSize;
            sizeX = Mathf.Clamp(sizeX, 1, maxSize.x);
            sizeY = Mathf.Clamp(sizeY, 1, maxSize.y);
        }
    }
}