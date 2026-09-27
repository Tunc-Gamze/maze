using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RunnerGame
{
    public sealed class TimedCoinEntry
    {
        public int Id { get; }
        public Cell Cell { get; }
        public double ExpiresAt { get; }
        internal TimedCoinEntry(int id, Cell cell, double expiresAt) { Id = id; Cell = cell; ExpiresAt = expiresAt; }
    }

    // Owns gameplay time and finite spawn opportunities, without Unity or global random state.
    public sealed class TimedCoinCycle
    {
        private readonly List<TimedCoinEntry> active = new List<TimedCoinEntry>();
        private readonly Cell[] plannedCells;
        private readonly int maxActive;
        private readonly double interval;
        private readonly double lifetime;
        private double nextSpawnAt;
        public ReadOnlyCollection<TimedCoinEntry> Active { get; }
        public double Elapsed { get; private set; }
        public int Spawned { get; private set; }
        public int Expired { get; private set; }
        public int Collected { get; private set; }
        public int Budget => plannedCells.Length;
        public int Pending => Budget - Spawned;
        public event Action<TimedCoinEntry> CoinSpawned;
        public event Action<int> CoinExpired;

        public TimedCoinCycle(LevelLayout map, CoinSpawnSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate(map);
            if (settings.mode != CoinBehavior.Timed) throw new ArgumentException("A timed cycle needs timed settings.");
            maxActive = settings.maxActive;
            interval = settings.spawnInterval;
            lifetime = settings.lifetime;
            nextSpawnAt = settings.initialDelay;
            Active = active.AsReadOnly();
            plannedCells = new Cell[settings.totalCoins];
            var cells = new List<Cell>(map.Coins);
            uint random = unchecked((uint)settings.seed);
            // Shuffle one bag, then cycle it: stable positions even when safety delays a spawn.
            for (int i = cells.Count - 1; i > 0; i--)
            {
                random = unchecked(random * 1664525u + 1013904223u);
                int other = (int)(((ulong)random * (uint)(i + 1)) >> 32);
                Cell saved = cells[i]; cells[i] = cells[other]; cells[other] = saved;
            }
            for (int i = 0; i < plannedCells.Length; i++) plannedCells[i] = cells[i % cells.Count];
        }

        public double Remaining(TimedCoinEntry entry) => Math.Max(0d, entry.ExpiresAt - Elapsed);

        public void Tick(double deltaSeconds, bool running, Func<Cell, bool> isSafe)
        {
            if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (!running) return;
            if (isSafe == null) throw new ArgumentNullException(nameof(isSafe));
            Elapsed += deltaSeconds;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i].ExpiresAt > Elapsed) continue;
                int id = active[i].Id;
                active.RemoveAt(i);
                Expired++;
                CoinExpired?.Invoke(id);
            }
            if (Pending == 0 || active.Count >= maxActive || Elapsed < nextSpawnAt) return;
            Cell cell = plannedCells[Spawned];
            foreach (var entry in active)
                if (entry.Cell.X == cell.X && entry.Cell.Y == cell.Y) return;
            if (!isSafe(cell)) return;
            var coin = new TimedCoinEntry(Spawned, cell, Elapsed + lifetime);
            active.Add(coin);
            Spawned++;
            // Never burst several unseen/expired coins after a long frame or blocked spawn.
            nextSpawnAt = Elapsed + interval;
            CoinSpawned?.Invoke(coin);
        }

        public bool TryCollect(int id)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Id != id || active[i].ExpiresAt <= Elapsed) continue;
                active.RemoveAt(i);
                Collected++;
                return true;
            }
            return false;
        }
    }
}
