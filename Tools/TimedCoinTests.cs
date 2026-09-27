using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RunnerGame;

internal static class TimedCoinTests
{
    private static int checks;
    private static void Check(bool ok, string message) { checks++; if (!ok) throw new Exception(message); }
    private static bool Safe(Cell cell) => true;
    private static CoinSpawnSettings Settings() => new CoinSpawnSettings {
        mode = CoinBehavior.Timed, totalCoins = 6, maxActive = 2,
        initialDelay = 1, spawnInterval = 1, lifetime = 4, warningSeconds = 1, seed = 123
    };

    private static string Position(Cell cell) => cell.X + "," + cell.Y;

    private static void Audit(TimedCoinCycle cycle, LevelLayout map)
    {
        Check(cycle.Budget == cycle.Pending + cycle.Active.Count + cycle.Expired + cycle.Collected, "Coin budget accounting mismatch");
        var cells = new HashSet<string>();
        foreach (var entry in cycle.Active)
        {
            Check(cells.Add(Position(entry.Cell)), "Active coins overlap");
            Check(map.ToText().Split('\n')[entry.Cell.Y][entry.Cell.X] == 'C', "Invalid spawn cell");
            Check(cycle.Remaining(entry) > 0, "Expired entry remained active");
        }
    }

    private static void Reject(LevelLayout map, Action<CoinSpawnSettings> change)
    {
        var settings = Settings(); change(settings);
        bool rejected = false;
        try { new TimedCoinCycle(map, settings); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid coin settings accepted");
    }

    private static List<string> CollectAll(LevelLayout map, CoinSpawnSettings settings, bool delay)
    {
        var cycle = new TimedCoinCycle(map, settings);
        var sequence = new List<string>();
        var score = new ScoreService(); score.Reset(cycle.Budget);
        cycle.CoinSpawned += entry => sequence.Add(entry.Id + ":" + Position(entry.Cell));
        for (int frame = 0; frame < 10000 && cycle.Collected < cycle.Budget; frame++)
        {
            cycle.Tick(0.25, true, cell => !delay || frame % 13 == 0);
            foreach (var entry in new List<TimedCoinEntry>(cycle.Active))
            {
                Check(cycle.TryCollect(entry.Id), "Live collection rejected");
                Check(score.TryCollect(entry.Id), "Valid coin score rejected");
                Check(!cycle.TryCollect(entry.Id) && !score.TryCollect(entry.Id), "Coin collected twice");
            }
            Audit(cycle, map);
        }
        Check(cycle.Collected == cycle.Budget && score.Score == settings.totalCoins * ScoreService.PointsPerCoin, "Finite budget not collectible");
        cycle.Tick(100000, true, Safe);
        Check(cycle.Pending == 0 && cycle.Active.Count == 0 && !cycle.TryCollect(settings.totalCoins), "Exhausted cycle spawned again");
        return sequence;
    }

    private static float Number(string text, string name)
    {
        var match = Regex.Match(text, @"(?m)^\s+" + name + @": (-?[\d.]+)\s*$");
        if (!match.Success) throw new Exception("Missing coin test configuration field " + name);
        return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    public static void Run(string levelDirectory)
    {
        var map = new LevelLayout(File.ReadAllText(Path.Combine(levelDirectory, "Level01.txt")));
        var settings = Settings();
        var cycle = new TimedCoinCycle(map, settings);
        cycle.Tick(0.5, true, Safe);
        Check(cycle.Spawned == 0, "Spawn before initial delay");
        cycle.Tick(500, false, Safe);
        Check(cycle.Elapsed == 0.5 && cycle.Spawned == 0, "Pause/calibration advanced time");
        cycle.Tick(0.5, true, cell => false);
        Check(cycle.Pending == 6 && cycle.Spawned == 0, "Blocked spawn spent budget");
        cycle.Tick(0, true, Safe);
        Check(cycle.Spawned == 1 && cycle.Active.Count == 1, "Safe spawn did not resume");
        int first = cycle.Active[0].Id;
        double remaining = cycle.Remaining(cycle.Active[0]);
        cycle.Tick(100, false, Safe);
        Check(cycle.Remaining(cycle.Active[0]) == remaining, "Paused lifetime changed");
        cycle.Tick(1, true, Safe);
        Check(cycle.Active.Count == 2, "Second spawn missing");
        cycle.Tick(1, true, Safe);
        Check(cycle.Active.Count == 2 && cycle.Spawned == 2, "MaxActive exceeded");
        int expiredEvents = 0;
        cycle.CoinExpired += id => expiredEvents++;
        cycle.Tick(2, true, Safe);
        Check(cycle.Expired == 1 && expiredEvents == 1 && !cycle.TryCollect(first), "Expiry boundary still collectable");
        Audit(cycle, map);
        for (int i = 0; i < 100; i++) { cycle.Tick(1, true, Safe); Audit(cycle, map); }
        Check(cycle.Expired == cycle.Budget && cycle.Active.Count == 0 && cycle.Pending == 0, "Uncollected budget recycled indefinitely");
        Check(expiredEvents == cycle.Budget, "Expiry did not notify each coin once");

        var normal = CollectAll(map, settings, false);
        var delayed = CollectAll(map, settings, true);
        Check(string.Join("|", normal) == string.Join("|", delayed), "Safety delay changed planned sequence");
        Check(string.Join("|", normal) == string.Join("|", CollectAll(map, settings, false)), "Restart changed seed sequence");
        var otherSettings = Settings(); otherSettings.seed = -999;
        Check(string.Join("|", normal) != string.Join("|", CollectAll(map, otherSettings, false)), "Different seed did not change this spawn sequence");
        var occupancySettings = Settings();
        occupancySettings.initialDelay = 0; occupancySettings.maxActive = map.Coins.Count; occupancySettings.lifetime = 100;
        var occupied = new TimedCoinCycle(map, occupancySettings);
        occupied.Tick(0, true, Safe);
        for (int i = 1; i < map.Coins.Count; i++) occupied.Tick(1, true, Safe);
        for (int i = 1; i < map.Coins.Count; i++) Check(occupied.TryCollect(i), "Occupancy setup collection failed");
        occupied.Tick(1, true, Safe);
        Check(occupied.Spawned == map.Coins.Count && occupied.Pending == 1, "Occupied cell was reused or spent budget");
        occupied.TryCollect(0); occupied.Tick(0, true, Safe);
        Check(occupied.Pending == 0 && occupied.Active.Count == 1, "Vacated cell did not accept the deferred coin");
        var largeFrame = new TimedCoinCycle(map, settings);
        largeFrame.Tick(1000, true, Safe);
        Check(largeFrame.Spawned == 1 && largeFrame.Remaining(largeFrame.Active[0]) == settings.lifetime, "Large frame caused burst/unseen expiry");

        Reject(map, s => s.mode = (CoinBehavior)99);
        Reject(map, s => s.totalCoins = 101);
        Reject(map, s => s.totalCoins = 0);
        Reject(map, s => s.maxActive = map.Coins.Count + 1);
        Reject(map, s => s.maxActive = 0);
        Reject(map, s => s.initialDelay = float.NaN);
        Reject(map, s => s.spawnInterval = 0);
        Reject(map, s => s.lifetime = float.PositiveInfinity);
        Reject(map, s => s.warningSeconds = s.lifetime);
        var empty = new LevelLayout("#####\n#S.G#\n#####");
        Reject(empty, s => { });

        // Real campaign settings, not just test fixtures. Preserve previous six score contexts.
        string[] parts = Regex.Split(File.ReadAllText(Path.Combine(levelDirectory, "GameConfig.asset")), @"(?m)^  - id: ");
        Check(parts.Length == 9, "Expected eight campaign levels");
        string[] oldIds = { "first-steps", "crossroads", "long-way", "maze-v1-forest", "maze-v1-crossing", "maze-v1-depths" };
        for (int i = 1; i <= 6; i++)
        {
            Check(parts[i].StartsWith(oldIds[i - 1] + "\n") || parts[i].StartsWith(oldIds[i - 1] + "\r\n"), "Previous level ID changed");
            Check(Number(parts[i], "mode") == 0, "Previous score context changed to timed coins");
        }
        for (int i = 7; i <= 8; i++)
        {
            string generation = parts[i].Split(new[] { "    coins:" }, StringSplitOptions.None)[0];
            string coinData = parts[i].Split(new[] { "    coins:" }, StringSplitOptions.None)[1];
            var layout = MazeGenerator.Generate(new MazeGenerationSettings {
                width = (int)Number(generation, "width"), height = (int)Number(generation, "height"),
                seed = (int)Number(generation, "seed"), coinCount = (int)Number(generation, "coinCount"),
                extraConnections = (int)Number(generation, "extraConnections")
            });
            var timed = new CoinSpawnSettings {
                mode = (CoinBehavior)(int)Number(coinData, "mode"), seed = (int)Number(coinData, "seed"),
                totalCoins = (int)Number(coinData, "totalCoins"), maxActive = (int)Number(coinData, "maxActive"),
                initialDelay = Number(coinData, "initialDelay"), spawnInterval = Number(coinData, "spawnInterval"),
                lifetime = Number(coinData, "lifetime"), warningSeconds = Number(coinData, "warningSeconds")
            };
            CollectAll(layout, timed, false);
            var idle = new TimedCoinCycle(layout, timed);
            for (int step = 0; step < 1000; step++)
            {
                idle.Tick(0.5, true, Safe);
                Check(idle.Active.Count <= timed.maxActive, "Campaign active cap exceeded");
                Audit(idle, layout);
            }
            Check(idle.Expired == timed.totalCoins && idle.Pending == 0, "Campaign idle cycle not finite");
            Console.WriteLine("M8 campaign level " + i + ": budget " + timed.totalCoins + ", max active " + timed.maxActive + ", lifetime " + timed.lifetime + "s");
        }
        Console.WriteLine("M8: " + checks + " timed coin data checks passed. No Unity runtime tested.");
    }
}
