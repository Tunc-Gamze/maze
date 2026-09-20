using System;
using System.Collections.Generic;
using System.Text;

namespace RunnerGame
{
    public static class MazeGenerator
    {
        private static readonly int[] Dx = { -1, 1, 0, 0 };
        private static readonly int[] Dy = { 0, 0, -1, 1 };

        // Version 1 is a published layout contract. Changes to its ordering/PRNG require
        // a new version and new level IDs if existing best scores must stay comparable.
        public static LevelLayout Generate(MazeGenerationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            var random = new StableRandom(settings.seed);
            var grid = new char[settings.width, settings.height];
            for (int y = 0; y < settings.height; y++)
            for (int x = 0; x < settings.width; x++) grid[x, y] = '#';

            var start = new Cell(1, 1);
            var stack = new Stack<Cell>();
            var choices = new List<Cell>(4);
            grid[start.X, start.Y] = '.';
            stack.Push(start);
            // Iterative randomized depth-first carving: a spanning tree over all rooms.
            while (stack.Count > 0)
            {
                Cell current = stack.Peek();
                choices.Clear();
                for (int direction = 0; direction < 4; direction++)
                {
                    int x = current.X + Dx[direction] * 2, y = current.Y + Dy[direction] * 2;
                    if (x > 0 && y > 0 && x < settings.width - 1 && y < settings.height - 1 && grid[x, y] == '#')
                        choices.Add(new Cell(x, y));
                }
                if (choices.Count == 0) { stack.Pop(); continue; }
                Cell next = choices[random.Next(choices.Count)];
                grid[(current.X + next.X) / 2, (current.Y + next.Y) / 2] = '.';
                grid[next.X, next.Y] = '.';
                stack.Push(next);
            }

            // Open only known interior connectors. Adding paths never disconnects the maze.
            var connectors = new List<Cell>();
            for (int y = 1; y < settings.height - 1; y++)
            for (int x = 1; x < settings.width - 1; x++)
                if ((x % 2 != y % 2) && grid[x, y] == '#') connectors.Add(new Cell(x, y));
            Shuffle(connectors, random);
            for (int i = 0; i < settings.extraConnections; i++)
                grid[connectors[i].X, connectors[i].Y] = '.';

            // Select the most distant room by actual shortest-path length AFTER adding loops.
            int[,] distances = Distances(grid, start);
            Cell goal = start;
            var rooms = new List<Cell>();
            for (int y = 1; y < settings.height; y += 2)
            for (int x = 1; x < settings.width; x += 2)
            {
                if (distances[x, y] > distances[goal.X, goal.Y]) goal = new Cell(x, y);
                rooms.Add(new Cell(x, y));
            }
            rooms.RemoveAll(cell => (cell.X == start.X && cell.Y == start.Y) || (cell.X == goal.X && cell.Y == goal.Y));
            Shuffle(rooms, random);
            for (int i = 0; i < settings.coinCount; i++) grid[rooms[i].X, rooms[i].Y] = 'C';
            grid[start.X, start.Y] = 'S';
            grid[goal.X, goal.Y] = 'G';

            var text = new StringBuilder();
            for (int y = 0; y < settings.height; y++)
            {
                if (y > 0) text.Append('\n');
                for (int x = 0; x < settings.width; x++) text.Append(grid[x, y]);
            }
            // Keep the same closed-boundary, unique marker and full connectivity checks
            // as authored maps; invalid generated data never reaches the scene builder.
            return new LevelLayout(text.ToString());
        }

        private static int[,] Distances(char[,] grid, Cell start)
        {
            int width = grid.GetLength(0), height = grid.GetLength(1);
            var distances = new int[width, height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) distances[x, y] = -1;
            var queue = new Queue<Cell>();
            distances[start.X, start.Y] = 0;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Cell cell = queue.Dequeue();
                for (int direction = 0; direction < 4; direction++)
                {
                    int x = cell.X + Dx[direction], y = cell.Y + Dy[direction];
                    if (x < 0 || y < 0 || x >= width || y >= height || grid[x, y] == '#' || distances[x, y] >= 0) continue;
                    distances[x, y] = distances[cell.X, cell.Y] + 1;
                    queue.Enqueue(new Cell(x, y));
                }
            }
            return distances;
        }

        private static void Shuffle(List<Cell> cells, StableRandom random)
        {
            for (int i = cells.Count - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                Cell value = cells[i];
                cells[i] = cells[other];
                cells[other] = value;
            }
        }

        private sealed class StableRandom
        {
            private uint state;
            public StableRandom(int seed)
            {
                // Avalanche adjacent seeds before the first branch choice.
                unchecked
                {
                    uint value = (uint)seed + 0x9e3779b9u;
                    value = (value ^ (value >> 16)) * 0x7feb352du;
                    value = (value ^ (value >> 15)) * 0x846ca68bu;
                    state = value ^ (value >> 16);
                }
            }
            public int Next(int exclusiveMax)
            {
                // Explicit 32-bit arithmetic is stable across Mono/IL2CPP and runtime versions.
                state = unchecked(state * 1664525u + 1013904223u);
                return (int)(((ulong)state * (uint)exclusiveMax) >> 32);
            }
        }
    }
}
