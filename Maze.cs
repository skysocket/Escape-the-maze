using System;
using UnityEngine;
using System.Collections.Generic;

//using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
//using System.Text;
using System.Drawing;

    public static class Extensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng)
        {
            var e = source.ToArray();
            for (var i = e.Length - 1; i >= 0; i--)
            {
                var swapIndex = rng.Next(i + 1);
                yield return e[swapIndex];
                e[swapIndex] = e[i];
            }
        }
 
        public static CellState OppositeWall(this CellState orig)
        {
            return (CellState)(((int) orig >> 2) | ((int) orig << 2)) & CellState.Initial;
        }
 
        public static bool HasFlag(this CellState cs,CellState flag)
        {
            return ((int)cs & (int)flag) != 0;
        }
    }
 
    public enum CellState
    {
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        Visited = 128,
        Initial = Top | Right | Bottom | Left,
    }
 
    public struct RemoveWallAction
    {
        public Point Neighbour;
        public CellState Wall;
    }
 
    public class MazeCells
    {
        public CellState[,] _cells;
        private readonly int _width;
        private readonly int _height;
        private readonly System.Random _rng;
 
        public MazeCells(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new CellState[width, height];
            for(var x=0; x<width; x++)
                for(var y=0; y<height; y++)
                    _cells[x, y] = CellState.Initial;
            _rng = new System.Random();
            VisitCell(_rng.Next(width), _rng.Next(height));
        }
 
        public CellState this[int x, int y]
        {
            get { return _cells[x,y]; }
            set { _cells[x,y] = value; }
        }
 
        public IEnumerable<RemoveWallAction> GetNeighbours(Point p)
        {
            if (p.X > 0) yield return new RemoveWallAction {Neighbour = new Point(p.X - 1, p.Y), Wall = CellState.Left};
            if (p.Y > 0) yield return new RemoveWallAction {Neighbour = new Point(p.X, p.Y - 1), Wall = CellState.Top};
            if (p.X < _width-1) yield return new RemoveWallAction {Neighbour = new Point(p.X + 1, p.Y), Wall = CellState.Right};
            if (p.Y < _height-1) yield return new RemoveWallAction {Neighbour = new Point(p.X, p.Y + 1), Wall = CellState.Bottom};
        }
 
        public void VisitCell(int x, int y)
        {
            this[x,y] |= CellState.Visited;
            foreach (var p in GetNeighbours(new Point(x, y)).Shuffle(_rng).Where(z => !(this[z.Neighbour.X, z.Neighbour.Y].HasFlag(CellState.Visited))))
            {
                this[x, y] -= p.Wall;
                this[p.Neighbour.X, p.Neighbour.Y] -= p.Wall.OppositeWall();
                VisitCell(p.Neighbour.X, p.Neighbour.Y);
            }
        }
 
    }

public class MazeDataGenerator
{
    public int[,] FromDimensions(int sizeRows, int sizeCols)    // 2
    {
        var maze = new MazeCells(sizeRows, sizeCols);
        int[,] mazeint = new int[sizeRows, sizeCols];

        int rMax = mazeint.GetUpperBound(0);
        int cMax = mazeint.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                mazeint[i , j] = (int)maze._cells[i , j];
            }
        }

        return mazeint;
    }
}


public class Maze : MonoBehaviour
{
    private MazeDataGenerator generator;

    void Start()
    {
        int rows = 13;
        int cols = 15;

        generator = new MazeDataGenerator();
        var cells = generator.FromDimensions(rows, cols);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if ((cells[i , j] & 1) != 0)  // top
                    drawSide(i , j , 0);
                if (((cells[i , j] & 2) != 0)  // right
                    && !((i==rows-1) && (j==cols-1)))  // except for top right wall to escape the maze
                    drawSide(i , j , 1);
                if ((cells[i , j] & 4) != 0)  // bottom
                    drawSide(i , j , 2);
                if ((cells[i , j] & 8) != 0)  // left
                    drawSide(i , j , 3);
            }
        }
    }

    private void drawSide(int x, int y, int side)
    {
        float side_x=0;
        float side_y=0;
        float scale_x=0;
        float scale_y=0;

        // These two lines will need to change depending
        // of if side = 0,1,2,3 (top, right, bottom, left)
        if (side == 0) { //top
            side_x = x;
            side_y = y - 0.5f;
            scale_x = 1.1f;
            scale_y = 0.1f;
        }
        if (side == 1) { //right
            side_x = x + 0.5f;
            side_y = y;
            scale_x = 0.1f;
            scale_y = 1.1f;
        }
        if (side == 2) { //bottom
            side_x = x;
            side_y = y + 0.5f;
            scale_x = 1.1f;
            scale_y = 0.1f;
        }
        if (side == 3) { //left
            side_x = x - 0.5f;
            side_y = y;
            scale_x = 0.1f;
            scale_y = 1.1f;
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(side_x, 0.5f, side_y);
        cube.transform.localScale = new Vector3(scale_x, 2, scale_y);  
    }
}
