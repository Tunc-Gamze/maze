using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunnerGame
{
    public static class SceneNavigator
    {
        public static int SelectedLevel { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { SelectedLevel = 0; Time.timeScale = 1f; }
        public static void Play(int index)
        {
            SelectedLevel = Mathf.Max(0, index);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Gameplay");
        }
        public static void Menu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
