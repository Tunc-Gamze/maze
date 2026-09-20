using System;
using UnityEngine;

namespace RunnerGame
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameConfig config = null;
        private void Start()
        {
            Time.timeScale = 1f;
            var camera = new GameObject("Menu Camera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UIFactory.Ink;
            if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;
            RectTransform root = UIFactory.Canvas("Main Menu UI");
            UIFactory.Panel("Background", root, UIFactory.Ink).SetAsFirstSibling();
            RectTransform card = UIFactory.Rect("Menu", root);
            UIFactory.Anchor(card, new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.92f));
            UIFactory.Vertical(card, 8f);
            UIFactory.Label(card, "TILT MAZE", 52, 70f);
            UIFactory.Label(card, "Yolunu bul. Coinleri topla. Hedefe ulaş.", 25, 52f);
            try
            {
                if (config == null) throw new InvalidOperationException("GameConfig reference is missing.");
                config.Validate();
                ProgressionService progress = SaveService.Progress(config);
                UIFactory.Button(card, "Devam et · Bölüm " + (progress.HighestUnlocked + 1),
                    () => SceneNavigator.Play(progress.HighestUnlocked));
                for (int i = 0; i < config.levels.Length; i++)
                {
                    int selected = i;
                    bool unlocked = progress.IsUnlocked(i);
                    string title = (i + 1) + " · " + config.levels[i].title + (unlocked ?
                        "   |   En iyi: " + progress.BestScore(i) : "   |   Kilitli");
                    UIFactory.Button(card, title, () =>
                    {
                        if (progress.IsUnlocked(selected)) SceneNavigator.Play(selected);
                    }).interactable = unlocked;
                }
                UIFactory.Label(card, "Mobil: telefonu eğ  •  Bilgisayar: WASD / ok tuşları", 22, 50f);
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
                UIFactory.Label(card, "Bölüm ayarları eksik: " + error.Message, 22, 100f);
            }
        }
    }
}
