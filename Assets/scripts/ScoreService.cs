using System;
using System.Collections.Generic;

namespace RunnerGame
{
    public sealed class ScoreService
    {
        public const int PointsPerCoin = 100;
        private readonly HashSet<int> collected = new HashSet<int>();
        public int Available { get; private set; }
        public int Coins => collected.Count;
        public int Score => Coins * PointsPerCoin;

        public void Reset(int available)
        {
            if (available < 0) throw new ArgumentOutOfRangeException(nameof(available));
            Available = available;
            collected.Clear();
        }

        public bool TryCollect(int id) => id >= 0 && id < Available && collected.Add(id);
    }
}
