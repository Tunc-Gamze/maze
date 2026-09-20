using UnityEngine;

namespace RunnerGame
{
    public sealed class SaveService : IProgressStore
    {
        private const string Prefix = "RunnerMaze.v1.";
        public int Read(string key, int fallback) => PlayerPrefs.GetInt(Prefix + key, fallback);
        public void Write(string key, int value) => PlayerPrefs.SetInt(Prefix + key, value);
        public void Flush() => PlayerPrefs.Save();

        public static ProgressionService Progress(GameConfig config)
        {
            var ids = new string[config.levels.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = config.levels[i].id;
            return new ProgressionService(ids, new SaveService());
        }

        public static float Sensitivity => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "sensitivity", 25) / 10f, 0.5f, 5f);
        public static void SetSensitivity(float value)
        {
            PlayerPrefs.SetInt(Prefix + "sensitivity", Mathf.RoundToInt(Mathf.Clamp(value, 0.5f, 5f) * 10f));
            PlayerPrefs.Save();
        }
    }
}
