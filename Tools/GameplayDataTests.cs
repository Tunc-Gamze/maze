using System;
using System.IO;
using System.Collections.Generic;
using RunnerGame;

internal static class GameplayDataTests
{
    private sealed class MemoryStore : IProgressStore
    {
        public readonly Dictionary<string, int> Values = new Dictionary<string, int>();
        public int Flushes;
        public int Read(string key, int fallback) => Values.TryGetValue(key, out int value) ? value : fallback;
        public void Write(string key, int value) => Values[key] = value;
        public void Flush() { Flushes++; }
    }
    private static int checks;
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
    }

    private static void Reject(string text)
    {
        bool rejected = false;
        try { new LevelLayout(text); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid map accepted");
    }

    public static void Main(string[] args)
    {
        string[] maps = Directory.GetFiles(args[0], "Level*.txt");
        Check(maps.Length == 3, "Expected three authored levels");
        foreach (string path in maps)
        {
            var map = new LevelLayout(File.ReadAllText(path));
            Check(map.Coins.Count > 0, "Expected authored coins: " + path);
            Console.WriteLine("Reachability validated: " + Path.GetFileName(path));
        }
        Reject("###\n#S#\n###");
        Reject("#####\n#S#G#\n#####");
        Reject("#####\n#S.G#\n#.#C#\n###");
        Reject("#####\n#S.G.\n#####");
        Reject("#####\n#SXG#\n#####");
        Reject("#######\n#S.G###\n#####C#\n#######");
        var score = new ScoreService();
        score.Reset(3);
        Check(score.TryCollect(1), "Coin not collected");
        Check(!score.TryCollect(1), "Duplicate coin counted");
        Check(!score.TryCollect(-1) && !score.TryCollect(3), "Invalid coin counted");
        Check(score.Score == 100 && score.Coins == 1, "Score mismatch");
        score.Reset(3);
        Check(score.Score == 0 && score.TryCollect(1), "Restart did not reset score");
        var store = new MemoryStore();
        var progress = new ProgressionService(new[] { "one", "two", "three" }, store);
        Check(progress.HighestUnlocked == 0 && !progress.IsUnlocked(1), "Initial lock failed");
        bool locked = false;
        try { progress.Complete(2, 100); } catch (InvalidOperationException) { locked = true; }
        Check(locked, "Locked completion accepted");
        progress.Complete(0, 200);
        Check(progress.HighestUnlocked == 1 && progress.BestScore(0) == 200, "Unlock/save failed");
        progress.Complete(0, 100);
        Check(progress.BestScore(0) == 200 && progress.HighestUnlocked == 1, "Replay regressed score or advanced unlock");
        progress.Complete(1, 0);
        Check(progress.IsUnlocked(2), "Zero-coin win must unlock next level");
        progress.Complete(2, 300);
        Check(progress.HighestUnlocked == 2 && !progress.IsUnlocked(3), "Final level bounds failed");
        var reloaded = new ProgressionService(new[] { "one", "two", "three" }, store);
        Check(reloaded.BestScore(2) == 300 && reloaded.HighestUnlocked == 2, "Progress reconstruction failed");
        Check(store.Flushes == 4, "Wins not flushed");
        progress.Complete(0, 500);
        Check(progress.BestScore(0) == 500, "Higher replay score not saved");
        bool negative = false;
        try { progress.Complete(0, -1); } catch (ArgumentOutOfRangeException) { negative = true; }
        Check(negative && progress.BestScore(0) == 500, "Negative score corrupted save");
        store.Values["level.two.best"] = -99;
        Check(progress.BestScore(1) == 0, "Malformed negative saved score not clamped");
        var extended = new ProgressionService(new[] { "one", "two", "three", "four", "five", "six" }, store);
        Check(extended.HighestUnlocked == 3 && !extended.IsUnlocked(4), "Appending levels broke completed campaign progression");
        Check(extended.BestScore(0) == 500 && extended.BestScore(2) == 300, "Appending levels lost existing best scores");
        extended.Complete(3, 0); extended.Complete(4, 0); extended.Complete(5, 200);
        var withTimed = new ProgressionService(new[] { "one", "two", "three", "four", "five", "six", "seven", "eight" }, store);
        Check(withTimed.HighestUnlocked == 6 && !withTimed.IsUnlocked(7), "Completed M7 campaign did not unlock first timed level");
        Check(withTimed.BestScore(0) == 500 && withTimed.BestScore(5) == 200, "Timed level extension changed existing best scores");
        Console.WriteLine(checks + " data checks passed; no Unity runtime behavior tested.");
        MazeGenerationTests.Run(args[0]);
        TimedCoinTests.Run(args[0]);
    }
}
