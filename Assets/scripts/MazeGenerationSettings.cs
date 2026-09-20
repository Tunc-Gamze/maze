using System;

namespace RunnerGame
{
    // Plain serializable data: usable by Unity and the standalone data tests.
    [Serializable]
    public sealed class MazeGenerationSettings
    {
        public int generatorVersion = 1;
        public int width = 11;
        public int height = 9;
        public int seed = 1001;
        public int coinCount = 6;
        public int extraConnections = 0;

        public void Validate()
        {
            if (generatorVersion != 1) throw new ArgumentException("Unsupported maze generator version.");
            // Odd dimensions include the closed outer wall. Bound work and geometry on mobile.
            if (width < 5 || width > 25 || height < 5 || height > 25 || width % 2 == 0 || height % 2 == 0)
                throw new ArgumentException("Maze width/height must be odd numbers between 5 and 25, including the boundary.");
            int columns = (width - 1) / 2, rows = (height - 1) / 2;
            if (coinCount < 0 || coinCount > columns * rows - 2)
                throw new ArgumentException("Coin count exceeds the available room centers, excluding start and goal.");
            if (extraConnections < 0 || extraConnections > (columns - 1) * (rows - 1))
                throw new ArgumentException("Extra connections exceed the available interior connections.");
        }
    }
}
