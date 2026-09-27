using System;

namespace RunnerGame
{
    public enum CoinBehavior { Static = 0, Timed = 1 }

    [Serializable]
    public sealed class CoinSpawnSettings
    {
        public CoinBehavior mode;
        public int seed = 8101;
        public int totalCoins = 10;
        public int maxActive = 3;
        public float initialDelay = 2f;
        public float spawnInterval = 6f;
        public float lifetime = 24f;
        public float warningSeconds = 5f;

        public int Budget(LevelLayout map) => mode == CoinBehavior.Static ? map.Coins.Count : totalCoins;

        public void Validate(LevelLayout map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (mode == CoinBehavior.Static) return;
            if (mode != CoinBehavior.Timed) throw new ArgumentException("Unknown coin mode.");
            if (totalCoins < 1 || totalCoins > 100 || maxActive < 1 || maxActive > totalCoins || maxActive > map.Coins.Count)
                throw new ArgumentException("Timed coins need a budget of 1-100 and enough distinct spawn cells for maxActive.");
            if (!Finite(initialDelay) || initialDelay < 0f || initialDelay > 60f ||
                !Finite(spawnInterval) || spawnInterval < 0.25f || spawnInterval > 120f ||
                !Finite(lifetime) || lifetime < 1f || lifetime > 300f ||
                !Finite(warningSeconds) || warningSeconds <= 0f || warningSeconds >= lifetime)
                throw new ArgumentException("Invalid timed coin delays, lifetime or warning duration.");
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
