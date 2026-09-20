using System;
using System.Collections.Generic;

namespace RunnerGame
{
    // Pure data: intentionally independent of Unity so generation can replace authored maps in M7.
    public struct Cell
    {
        public int X;
        public int Y;
        public Cell(int x, int y) { X = x; Y = y; }
    }

    public sealed class LevelLayout
    {
        private readonly string[] rows;
        public int Width => rows[0].Length;
        public int Height => rows.Length;
        public Cell Start { get; private set; }
        public Cell Goal { get; private set; }
        public readonly List<Cell> Coins = new List<Cell>();
        public bool IsWall(int x, int y) => rows[y][x] == '#';

        public LevelLayout(string text)
        {
            rows = text.Replace("\r", "").Trim().Split('\n');
            if (rows.Length < 3 || Width < 3) throw new ArgumentException("Map is too small.");
            int starts = 0, goals = 0, walkable = 0;
            for (int y = 0; y < Height; y++)
            {
                if (rows[y].Length != Width) throw new ArgumentException("Map rows must have equal width.");
                for (int x = 0; x < Width; x++)
                {
                    char tile = rows[y][x];
                    if ("#.SCG".IndexOf(tile) < 0) throw new ArgumentException("Unknown map tile: " + tile);
                    if ((x == 0 || y == 0 || x == Width - 1 || y == Height - 1) && tile != '#')
                        throw new ArgumentException("Map boundary must be closed.");
                    if (tile != '#') walkable++;
                    if (tile == 'S') { Start = new Cell(x, y); starts++; }
                    if (tile == 'G') { Goal = new Cell(x, y); goals++; }
                    if (tile == 'C') Coins.Add(new Cell(x, y));
                }
            }
            if (starts != 1 || goals != 1) throw new ArgumentException("Map needs exactly one start and goal.");
            var visited = new bool[Width, Height];
            var queue = new Queue<Cell>();
            queue.Enqueue(Start);
            visited[Start.X, Start.Y] = true;
            int reached = 0;
            int[] dx = { -1, 1, 0, 0 }, dy = { 0, 0, -1, 1 };
            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();
                reached++;
                for (int i = 0; i < 4; i++)
                {
                    int x = current.X + dx[i], y = current.Y + dy[i];
                    if (x < 0 || y < 0 || x >= Width || y >= Height || visited[x, y] || IsWall(x, y)) continue;
                    visited[x, y] = true;
                    queue.Enqueue(new Cell(x, y));
                }
            }
            if (reached != walkable) throw new ArgumentException("Every walkable cell, goal and coin must be reachable.");
        }
    }
}
