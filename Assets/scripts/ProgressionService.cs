using System;
using System.Collections.Generic;

namespace RunnerGame
{
    public interface IProgressStore
    {
        int Read(string key, int fallback);
        void Write(string key, int value);
        void Flush();
    }

    public sealed class ProgressionService
    {
        private readonly string[] ids;
        private readonly IProgressStore store;
        public ProgressionService(string[] levelIds, IProgressStore storage)
        {
            if (levelIds == null || levelIds.Length == 0 || storage == null)
                throw new ArgumentException("Progression needs levels and storage.");
            var unique = new HashSet<string>();
            foreach (string id in levelIds)
                if (string.IsNullOrEmpty(id) || !unique.Add(id)) throw new ArgumentException("Invalid level ID.");
            ids = (string[])levelIds.Clone();
            store = storage;
        }

        // Completion is keyed by stable level ID, not by a scene or display name.
        public int HighestUnlocked
        {
            get
            {
                int highest = 0;
                while (highest + 1 < ids.Length && IsComplete(highest)) highest++;
                return highest;
            }
        }
        public bool IsUnlocked(int index) => index >= 0 && index < ids.Length && index <= HighestUnlocked;
        public bool IsComplete(int index) => store.Read(Key(index, "complete"), 0) == 1;
        public int BestScore(int index) => Math.Max(0, store.Read(Key(index, "best"), 0));

        public void Complete(int index, int score)
        {
            if (!IsUnlocked(index)) throw new InvalidOperationException("Cannot complete a locked level.");
            if (score < 0) throw new ArgumentOutOfRangeException(nameof(score));
            store.Write(Key(index, "best"), Math.Max(BestScore(index), score));
            store.Write(Key(index, "complete"), 1);
            store.Flush();
        }

        private string Key(int index, string field)
        {
            if (index < 0 || index >= ids.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return "level." + ids[index] + "." + field;
        }
    }
}
