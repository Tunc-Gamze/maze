using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RunnerGame;

internal static class MazeGenerationTests
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }

    private static void Reject(MazeGenerationSettings settings)
    {
        bool rejected = false;
        try { MazeGenerator.Generate(settings); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid generator settings accepted");
    }

    // Independent flood fill over the emitted tile text, not the generator's distance helper.
    private static int Audit(LevelLayout map, MazeGenerationSettings settings)
    {
        string[] rows = map.ToText().Split('\n');
        Check(map.Width == settings.width && map.Height == settings.height, "Wrong dimensions");
        Check(map.Start.X == 1 && map.Start.Y == 1, "Unexpected start");
        Check(map.Coins.Count == settings.coinCount, "Wrong coin count");
        var distance = new Dictionary<int, int>();
        var queue = new Queue<int>();
        int start = map.Start.Y * map.Width + map.Start.X;
        distance.Add(start, 0);
        queue.Enqueue(start);
        int[] offsets = { -1, 1, -map.Width, map.Width };
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (int offset in offsets)
            {
                int next = current + offset;
                int x = next % map.Width, y = next / map.Width;
                if (next < 0 || next >= map.Width * map.Height || rows[y][x] == '#' || distance.ContainsKey(next)) continue;
                distance.Add(next, distance[current] + 1);
                queue.Enqueue(next);
            }
        }
        int rooms = 0, paths = 0, maxDistance = 0;
        for (int y = 0; y < map.Height; y++)
        for (int x = 0; x < map.Width; x++)
        {
            char tile = rows[y][x];
            if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
                Check(tile == '#', "Open boundary");
            if (tile == '#') continue;
            paths++;
            Check(distance.ContainsKey(y * map.Width + x), "Unreachable floor");
            if (x % 2 == 1 && y % 2 == 1)
            {
                rooms++;
                maxDistance = Math.Max(maxDistance, distance[y * map.Width + x]);
            }
            else Check(tile == '.', "Coin/start/goal placed in a narrow connector");
        }
        int expectedRooms = ((map.Width - 1) / 2) * ((map.Height - 1) / 2);
        Check(rooms == expectedRooms, "Room missing");
        Check(paths == 2 * rooms - 1 + settings.extraConnections, "Incorrect tree/extra connection count");
        int goalDistance = distance[map.Goal.Y * map.Width + map.Goal.X];
        Check(goalDistance == maxDistance && goalDistance > 0, "Goal is not the farthest distinct room");
        var coins = new HashSet<int>();
        foreach (Cell coin in map.Coins)
        {
            Check(rows[coin.Y][coin.X] == 'C', "Coin marker mismatch");
            Check(coins.Add(coin.Y * map.Width + coin.X), "Duplicate coin cell");
            Check(!(coin.X == map.Start.X && coin.Y == map.Start.Y) &&
                !(coin.X == map.Goal.X && coin.Y == map.Goal.Y), "Coin overlaps start/goal");
        }
        return goalDistance;
    }

    private static int Number(string section, string name)
    {
        Match match = Regex.Match(section, @"(?m)^\s+" + name + @": (-?\d+)\s*$");
        if (!match.Success) throw new Exception("Missing campaign field " + name);
        return int.Parse(match.Groups[1].Value);
    }

    public static void Run(string levelDirectory)
    {
        int[,] sizes = { { 5, 5 }, { 9, 9 }, { 11, 9 }, { 13, 11 }, { 15, 13 }, { 25, 25 }, { 5, 25 }, { 25, 5 } };
        var variety = new HashSet<string>();
        for (int size = 0; size < sizes.GetLength(0); size++)
        for (int seed = -64; seed < 64; seed++)
        {
            int width = sizes[size, 0], height = sizes[size, 1];
            int columns = (width - 1) / 2, rows = (height - 1) / 2;
            var settings = new MazeGenerationSettings {
                width = width, height = height, seed = seed,
                coinCount = (seed + 64) % (columns * rows - 1),
                extraConnections = (seed + 64) % ((columns - 1) * (rows - 1) + 1)
            };
            LevelLayout map = MazeGenerator.Generate(settings);
            Audit(map, settings);
            Check(map.ToText() == MazeGenerator.Generate(settings).ToText(), "Same seed/settings changed the maze");
            if (size == 1)
                variety.Add(MazeGenerator.Generate(new MazeGenerationSettings {
                    width = 9, height = 9, seed = seed, coinCount = 0, extraConnections = 0
                }).ToText());
        }
        Console.WriteLine("Distinct 9x9 layouts across 128 seeds: " + variety.Count);
        Check(variety.Count > 100, "Seeds do not produce enough variation");

        var extreme = new MazeGenerationSettings { width = 5, height = 5, coinCount = 2, extraConnections = 1 };
        foreach (int seed in new[] { int.MinValue, int.MaxValue, 0 })
        {
            extreme.seed = seed;
            Audit(MazeGenerator.Generate(extreme), extreme);
        }
        var coinSettings = new MazeGenerationSettings { seed = 2026, coinCount = 0 };
        string noCoins = MazeGenerator.Generate(coinSettings).ToText();
        coinSettings.coinCount = 18;
        Check(noCoins == MazeGenerator.Generate(coinSettings).ToText().Replace('C', '.'), "Coin count changed the route");

        Reject(null);
        Reject(new MazeGenerationSettings { generatorVersion = 2 });
        Reject(new MazeGenerationSettings { width = 4 });
        Reject(new MazeGenerationSettings { width = 26 });
        Reject(new MazeGenerationSettings { width = 27 });
        Reject(new MazeGenerationSettings { height = -1 });
        Reject(new MazeGenerationSettings { height = 10 });
        Reject(new MazeGenerationSettings { coinCount = -1 });
        Reject(new MazeGenerationSettings { coinCount = int.MaxValue });
        Reject(new MazeGenerationSettings { extraConnections = -1 });
        Reject(new MazeGenerationSettings { extraConnections = int.MaxValue });

        string config = File.ReadAllText(Path.Combine(levelDirectory, "GameConfig.asset"));
        string[] sections = Regex.Split(config, @"(?m)^  - id: ");
        Check(sections.Length >= 7, "Expected the original six campaign levels");
        string[] originalIds = { "first-steps", "crossroads", "long-way" };
        for (int i = 0; i < 3; i++)
        {
            Check(sections[i + 1].StartsWith(originalIds[i] + "\n") || sections[i + 1].StartsWith(originalIds[i] + "\r\n"), "Existing level ID/order changed");
            Check(Number(sections[i + 1], "source") == 0, "Existing map source changed");
        }
        int previousGoalDistance = 0;
        for (int i = 4; i <= 6; i++)
        {
            string part = sections[i];
            Check(Number(part, "source") == 1, "New campaign level is not generated");
            var settings = new MazeGenerationSettings {
                generatorVersion = Number(part, "generatorVersion"), width = Number(part, "width"),
                height = Number(part, "height"), seed = Number(part, "seed"),
                coinCount = Number(part, "coinCount"), extraConnections = Number(part, "extraConnections")
            };
            int length = Audit(MazeGenerator.Generate(settings), settings);
            Check(length > previousGoalDistance, "Curated generated campaign paths should increase in length");
            previousGoalDistance = length;
            Console.WriteLine("Campaign level " + i + ": " + settings.width + "x" + settings.height +
                ", seed " + settings.seed + ", shortest goal path " + length + " tiles, coins " + settings.coinCount);
        }

        var golden = new MazeGenerationSettings { width = 9, height = 9, seed = 2026, coinCount = 5, extraConnections = 2 };
        using (SHA256 hash = SHA256.Create())
        {
            string digest = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(MazeGenerator.Generate(golden).ToText()))).Replace("-", "").ToLowerInvariant();
            Console.WriteLine("Generator v1 reference SHA256: " + digest);
            Check(digest == "78375956501410fae2e460e072f3f3b67d4d0fed41aeaa0ef5973f651719bf1b", "Generator v1 changed its published layout contract");
        }
        Console.WriteLine("M7: 1024 seed/size/coin/connection combinations plus edge/campaign cases; " + checks + " invariant checks passed. No Unity runtime tested.");
    }
}
