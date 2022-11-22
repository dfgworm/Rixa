using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Generation;

[TestFixture]
public class CellCursorTest
{
    [SetUp]
    public void Setup()
    {
        EcsStatic.Load();
    }

    [Test]
    public void CellBasics()
    {
        var c1 = new Cell(1,4,1,2);
        var c2 = new Cell(1,4,1,2);
        Assert.IsTrue(c1 == c2);
        c2 = new Cell(2, 4, 1, 2);
        Assert.IsFalse(c1 == c2);
        c2 = new Cell(1, 4, 4, 2);
        Assert.IsFalse(c1 == c2);

        var neighbours = c1.GetNeighbours();
        Assert.AreEqual(4, neighbours.Count);
        foreach (var c in neighbours)
            Assert.AreEqual(1, (c.pos - c1.pos).magnitude);
    }

    [Test]
    public void CellGetAll()
    {
        var cell = new Cell(0,0, 6,8);
        var pickSize = Vector2Int.one;
        var cells = cell.GetAllCells(pickSize);
        Assert.AreEqual(cell.size.x * cell.size.y, cells.Count);
        foreach (var c in cells) {
            Assert.IsTrue(c.pos.x >= 0 && c.pos.x < cell.size.x);
            Assert.IsTrue(c.pos.y >= 0 && c.pos.y < cell.size.y);
            Assert.AreEqual(pickSize, c.size);
        }

        cell = new Cell(4, 1, 4, 6);
        cells = cell.GetAllCells(pickSize);
        Assert.AreEqual(cell.size.x * cell.size.y, cells.Count);
        foreach (var c in cells)
        {
            Assert.IsTrue(c.pos.x >= cell.pos.x && c.pos.x < cell.pos.x + cell.size.x);
            Assert.IsTrue(c.pos.y >= cell.pos.y && c.pos.y < cell.pos.y + cell.size.y);
            Assert.AreEqual(pickSize, c.size);
        }

        cell = new Cell(4, 1, 4, 6);
        pickSize = new Vector2Int(3,2);
        cells = cell.GetAllCells(pickSize);
        Assert.AreEqual((cell.size.x-pickSize.x+1) * (cell.size.y-pickSize.y+1), cells.Count);
        foreach (var c in cells)
        {
            Assert.IsTrue(c.pos.x >= cell.pos.x && c.pos.x < cell.pos.x + cell.size.x - pickSize.x+1);
            Assert.IsTrue(c.pos.y >= cell.pos.y && c.pos.y < cell.pos.y + cell.size.y - pickSize.y+1);
            Assert.AreEqual(pickSize, c.size);
            foreach (var _c in c.GetAllCells())
            {
                Assert.IsTrue(_c.pos.x >= c.pos.x && _c.pos.x < c.pos.x+c.size.x);
                Assert.IsTrue(_c.pos.y >= c.pos.y && _c.pos.y < c.pos.y+c.size.y);
            }
        }
    }
    [Test]
    public void CursorBasics()
    {
        var selectable = new TestSelectable();
        selectable.Size = new Vector2Int(10, 5);
        var cursor = new CellCursor(selectable);
        Assert.AreEqual(new Cell(0, 0, 1, 1), cursor.Cell);

        cursor.Cell = new Cell(0, 0, 15, 9);
        Assert.AreEqual(new Cell(Vector2Int.zero, selectable.Size), cursor.Cell);

        cursor.Reset();
        Assert.AreEqual(new Cell(0, 0, 1, 1), cursor.Cell);

        cursor.Cell = new Cell(1, 2, 15, 9);
        Assert.AreEqual(new Cell(1, 2, selectable.Size.x - 1, selectable.Size.y - 2), cursor.Cell);

        cursor.SelectAll();
        Assert.AreEqual(new Cell(Vector2Int.zero, selectable.Size), cursor.Cell);

        selectable.Size = new Vector2Int(5, 8);
        cursor.SelectAll();
        Assert.AreEqual(new Cell(Vector2Int.zero, selectable.Size), cursor.Cell);

    }
    [Test]
    public void CursorUnoccupied()
    {
        var selectable = new TestSelectable();
        selectable.Size = new Vector2Int(10, 5);
        var cursor = new CellCursor(selectable);

        var cells = cursor.Cell.GetAllCells();
        var unoccupied = cursor.GetAllUnoccupiedCells();
        Assert.AreEqual(cells.Count, unoccupied.Count);
        foreach (var c in cells)
            Assert.IsTrue(unoccupied.Contains(c));

        cells = cursor.Cell.GetAllCells(new Vector2Int(2,3));
        unoccupied = cursor.GetAllUnoccupiedCells(new Vector2Int(2, 3));
        Assert.AreEqual(cells.Count, unoccupied.Count);
        foreach (var c in cells)
            Assert.IsTrue(unoccupied.Contains(c));


        //testing unoccupied
        var cell = new Cell(2,3);
        selectable.occupied.Add(cell);
        Assert.IsTrue(cursor.IsOccupied(cell));
        Assert.IsTrue(cursor.IsOccupied(new Cell(1,1,3,3)));
        Assert.IsTrue(cursor.IsOccupied(cursor.SelectAll().Cell));

        cells = cursor.SelectAll().Cell.GetAllCells();
        unoccupied = cursor.SelectAll().GetAllUnoccupiedCells();
        Assert.AreEqual(cells.Count-1, unoccupied.Count);
        Assert.IsFalse(unoccupied.Contains(cell));

        //test unoccupied with bigger size
        cells = cursor.SelectAll().Cell.GetAllCells(new Vector2Int(2,2));
        unoccupied = cursor.SelectAll().GetAllUnoccupiedCells(new Vector2Int(2,2));
        Assert.AreEqual(cells.Count - 4, unoccupied.Count);
        foreach(var c in unoccupied)
            Assert.IsFalse(c.GetAllCells().Contains(cell));
    }

    [Test]
    public void CursorChange()
    {

        var selectable = new TestSelectable();
        selectable.Size = new Vector2Int(13, 8);
        var cursor = new CellCursor(selectable);

        //moving
        cursor.MoveToX(4);
        Assert.AreEqual(new Cell(4, 0, 1, 1), cursor.Cell);

        cursor.MoveToY(6);
        Assert.AreEqual(new Cell(4, 6, 1, 1), cursor.Cell);

        cursor.MoveTo(2,3);
        Assert.AreEqual(new Cell(2, 3, 1, 1), cursor.Cell);

        cursor.MoveTo(15, 10);
        Assert.AreEqual(new Cell(selectable.Size.x-1, selectable.Size.y-1, 1, 1), cursor.Cell);

        cursor.Reset();
        cursor.Move(direction.up, 1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Move(direction.up, 1);
        Assert.AreEqual(new Cell(0, 2, 1, 1), cursor.Cell);
        cursor.Move(direction.down, 1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Move(direction.left, 1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Move(direction.right, 2);
        Assert.AreEqual(new Cell(2, 1, 1, 1), cursor.Cell);
        cursor.Move(direction.right, 2);
        Assert.AreEqual(new Cell(4, 1, 1, 1), cursor.Cell);

        //i have avoided tests with moving area large than (1,1) outside of bounds because i am unsure of how it is supposed to work
        //i think as of now it will place the area and then clip it's size
        
        //expanding/contracting
        cursor.Reset();
        cursor.Expand(direction.up, 2);
        Assert.AreEqual(new Cell(0, 0, 1, 3), cursor.Cell);
        cursor.Expand(direction.down, 2);
        Assert.AreEqual(new Cell(0, 0, 1, 3), cursor.Cell);
        cursor.Expand(direction.right, 1);
        Assert.AreEqual(new Cell(0, 0, 2, 3), cursor.Cell);
        cursor.Expand(direction.left, 1);
        Assert.AreEqual(new Cell(0, 0, 2, 3), cursor.Cell);
        cursor.Expand(direction.right, -1);
        Assert.AreEqual(new Cell(0, 0, 1, 3), cursor.Cell);
        cursor.Expand(direction.up, -1);
        Assert.AreEqual(new Cell(0, 0, 1, 2), cursor.Cell);
        cursor.Expand(direction.down, -1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Expand(direction.down, -1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Expand(direction.left, -1);
        Assert.AreEqual(new Cell(0, 1, 1, 1), cursor.Cell);
        cursor.Expand(direction.right, 1)
            .Expand(direction.left, -1);
        Assert.AreEqual(new Cell(1, 1, 1, 1), cursor.Cell);
        cursor.Expand(direction.left, 1);
        Assert.AreEqual(new Cell(0, 1, 2, 1), cursor.Cell);
        cursor.Expand(direction.right, 200);
        Assert.AreEqual(new Cell(0, 1, selectable.Size.x, 1), cursor.Cell);
        cursor.Expand(direction.up, 200);
        Assert.AreEqual(new Cell(0, 1, selectable.Size.x, selectable.Size.y-1), cursor.Cell);

        cursor.Reset().MoveTo(5, 3)
            .Expand(direction.horizontal, 1);
        Assert.AreEqual(new Cell(4, 3, 3, 1), cursor.Cell);
        cursor.Expand(direction.vertical, 1);
        Assert.AreEqual(new Cell(4, 2, 3, 3), cursor.Cell);
        cursor.Expand(direction.vertical, -1);
        Assert.AreEqual(new Cell(4, 3, 3, 1), cursor.Cell);
        cursor.Expand(direction.vertical, -1);
        Assert.AreEqual(new Cell(4, 3, 3, 1), cursor.Cell);
        cursor.Expand(direction.horizontal, -1);
        Assert.AreEqual(new Cell(5, 3, 1, 1), cursor.Cell);

        cursor.Reset().MoveTo(0, 3)
            .Expand(direction.horizontal, 1);
        Assert.AreEqual(new Cell(0, 3, 2, 1), cursor.Cell);

        cursor.Reset().MoveTo(5, 3)
            .Expand(direction.all, 1);
        Assert.AreEqual(new Cell(4, 2, 3, 3), cursor.Cell);
        cursor.Expand(direction.all, -1);
        Assert.AreEqual(new Cell(5, 3, 1, 1), cursor.Cell);
        cursor.Reset().MoveTo(0, 0)
            .Expand(direction.all, 1);
        Assert.AreEqual(new Cell(0, 0, 2, 2), cursor.Cell);
        cursor.Reset().MoveTo(selectable.Size.x-1, selectable.Size.y-1)
            .Expand(direction.all, 1);
        Assert.AreEqual(new Cell(selectable.Size.x-2, selectable.Size.y-2, 2, 2), cursor.Cell);

        //MoveCenterTo
        // even number center is considered floored, e.g. left/down is prioritized
        cursor.Reset()
            .MoveCenterTo(3, 5);
        Assert.AreEqual(new Cell(3, 5, 1, 1), cursor.Cell);
        cursor.Reset().Expand(direction.up, 2)
            .MoveCenterTo(3, 5);
        Assert.AreEqual(new Cell(3, 4, 1, 3), cursor.Cell);
        cursor.Reset().Expand(direction.up, 2).Expand(direction.right, 2)
            .MoveCenterTo(3, 5);
        Assert.AreEqual(new Cell(2, 4, 3, 3), cursor.Cell);
        cursor.Reset().Expand(direction.up, 1)
            .MoveCenterTo(3, 5);
        Assert.AreEqual(new Cell(3, 5, 1, 2), cursor.Cell);
        cursor.Reset().Expand(direction.right, 1)
            .MoveCenterTo(3, 5);
        Assert.AreEqual(new Cell(3, 5, 2, 1), cursor.Cell);

        //MoveToEdge
        cursor.Reset()
            .MoveToEdge(direction.up);
        Assert.AreEqual(new Cell(0, selectable.Size.y - 1, 1, 1), cursor.Cell);
        cursor.MoveToEdge(direction.down);
        Assert.AreEqual(new Cell(0, 0, 1, 1), cursor.Cell);
        cursor.Reset()
            .MoveToEdge(direction.right);
        Assert.AreEqual(new Cell(selectable.Size.x - 1, 0, 1, 1), cursor.Cell);
        cursor.MoveToEdge(direction.left);
        Assert.AreEqual(new Cell(0, 0, 1, 1), cursor.Cell);

    }

    [Test]
    public void CursorGettingCells()
    {

        var selectable = new TestSelectable();
        selectable.Size = new Vector2Int(13, 8);
        var cursor = new CellCursor(selectable);


        //PickRandom
        Cell cell;
        for (int i = 0; i < 500; i++)
        {
            Assert.IsTrue(cursor.SelectAll().PickRandom(Vector2Int.one, out cell));
            Assert.IsTrue(cursor.Cell.GetAllCells().Contains(cell));
            Assert.AreEqual(Vector2Int.one, cell.size);
        }
        Assert.IsTrue(cursor.Reset().MoveTo(1, 1).PickRandom(Vector2Int.one, out cell));
        Assert.AreEqual(cursor.Cell, cell);
        selectable.occupied.Add(new Cell(1, 1));
        Assert.IsTrue(cursor.Reset().MoveTo(1, 1).PickRandom(Vector2Int.one, out cell, false));
        Assert.IsFalse(cursor.Reset().MoveTo(1, 1).PickRandom(Vector2Int.one, out cell, true));


        //FillGetCells
        selectable.occupied.Clear();
        var cells = cursor.Reset().FillGetCells();
        foreach (var c in cursor.SelectAll().Cell.GetAllCells())
            Assert.IsTrue(cells.Contains(c));
        selectable.occupied.Add(new Cell(1, 1));
        cells = cursor.Reset().FillGetCells();
        foreach (var c in cursor.SelectAll().GetAllUnoccupiedCells())
            Assert.IsTrue(cells.Contains(c));
        selectable.occupied.Add(new Cell(0, 1));
        selectable.occupied.Add(new Cell(1, 0));
        cells = cursor.Reset().FillGetCells();
        Assert.AreEqual(1, cells.Count);
        Assert.IsTrue(cells.Contains(new Cell(0, 0)));
        selectable.occupied.Add(new Cell(0, 0));
        cells = cursor.Reset().FillGetCells();
        Assert.AreEqual(0, cells.Count);
    }

    [TearDown]
    public void TearDown()
    {
        EcsStatic.Unload();
    }
    class TestSelectable : ICellSelectable
    {
        public Vector2Int Size { get; set; }
        public HashSet<Cell> occupied = new HashSet<Cell>();
        public bool IsOccupied(Cell cell)
        {
            if (cell.size.x == 1 && cell.size.y == 1)
                return occupied.Contains(cell);
            foreach (var c in cell.GetAllCells())
                if (occupied.Contains(c))
                    return true;
            return false;
        }
    }
}
