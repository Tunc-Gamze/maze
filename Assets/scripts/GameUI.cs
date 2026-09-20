using UnityEngine;
using UnityEngine.UI;

namespace RunnerGame
{
    public sealed class GameUI : MonoBehaviour
    {
        private GameSession session;
        private RectTransform overlay;
        private RectTransform card;
        private Text status;
        private Text hud;
        private Text pauseStatus;

        public void Initialize(GameSession owner)
        {
            session = owner;
            RectTransform root = UIFactory.Canvas("Gameplay UI");
            RectTransform bar = UIFactory.Panel("HUD", root, UIFactory.Ink);
            UIFactory.Anchor(bar, new Vector2(0.02f, 0.87f), new Vector2(0.98f, 0.98f));
            hud = UIFactory.Label(bar, "Bölüm " + (owner.LevelIndex + 1), 28);
            UIFactory.Anchor((RectTransform)hud.transform, Vector2.zero, new Vector2(0.76f, 1f));
            Button pause = UIFactory.Button(bar, "Duraklat", session.Pause);
            UIFactory.Anchor((RectTransform)pause.transform, new Vector2(0.8f, 0.08f), new Vector2(0.98f, 0.92f));
            status = UIFactory.Label(root, "", 23);
            UIFactory.Anchor((RectTransform)status.transform, new Vector2(0.04f, 0.01f), new Vector2(0.96f, 0.09f));
            overlay = UIFactory.Panel("Overlay", root, new Color(0f, 0f, 0f, 0.78f));
            card = UIFactory.Panel("Dialog", overlay, UIFactory.Ink);
            UIFactory.Anchor(card, new Vector2(0.22f, 0.06f), new Vector2(0.78f, 0.94f));
            UIFactory.Vertical(card, 10f);
            overlay.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (session == null || session.Player == null || status == null) return;
            PlayerInput input = session.Player.Controls;
            if (pauseStatus != null)
                pauseStatus.text = input.IsCalibrating ? "Telefonu sabit tutun… Kalibrasyon sürüyor." :
                    !string.IsNullOrEmpty(session.SaveWarning) ? session.SaveWarning :
                    input.UsesTilt ? "Rahat tutuş için yeniden kalibre edebilirsiniz." : "Klavye kontrolü etkin.";
            status.text = input.IsCalibrating ? "Kalibrasyon: telefonu rahat konumda sabit tutun…" :
                input.SensorUnavailable ? "Eğim sensörü bulunamadı. Bu cihazda tilt kontrolü kullanılamıyor." :
                input.UsesTilt ? "Telefonu eğerek yeşil hedefe ulaşın. Coinler isteğe bağlıdır." :
                "WASD / ok tuşları: hareket   •   Esc: duraklat   •   C: kalibrasyon";
        }

        public void HideDialog() => overlay.gameObject.SetActive(false);

        public void RefreshHUD()
        {
            hud.text = "Bölüm " + (session.LevelIndex + 1) + "   •   Coin " + session.Score.Coins +
                "/" + session.Score.Available + "   •   Skor " + session.Score.Score;
        }

        private void BeginDialog(string title)
        {
            UIFactory.Clear(card);
            overlay.gameObject.SetActive(true);
            UIFactory.Label(card, title, 38, 56f);
        }

        public void ShowPause()
        {
            BeginDialog("Duraklatıldı");
            pauseStatus = UIFactory.Label(card, "", 22, 40f);
            UIFactory.Button(card, "Devam et", session.Resume);
            UIFactory.Button(card, "Yeniden kalibre et", () => session.Player.Controls.Calibrate());
            UIFactory.Button(card, "Hassasiyet: " + session.Player.Controls.Sensitivity.ToString("0.0"), () =>
            {
                session.CycleSensitivity();
                ShowPause();
            });
            UIFactory.Button(card, "Bölümü yeniden başlat", session.Restart);
            UIFactory.Button(card, "Ana menü", SceneNavigator.Menu);
        }

        public void ShowWin()
        {
            BeginDialog("Hedefe ulaştın!");
            UIFactory.Label(card, "Bölüm " + (session.LevelIndex + 1) + " tamamlandı.");
            UIFactory.Label(card, "Coin: " + session.Score.Coins + "/" + session.Score.Available +
                "   •   Skor: " + session.Score.Score, 28, 52f);
            UIFactory.Label(card, string.IsNullOrEmpty(session.SaveWarning) ?
                "En iyi skor: " + session.Progress.BestScore(session.LevelIndex) : session.SaveWarning, 25);
            if (session.HasNextLevel)
                UIFactory.Button(card, "Sonraki bölüm", session.NextLevel).interactable =
                    session.Progress.IsUnlocked(session.LevelIndex + 1);
            else UIFactory.Label(card, "Tüm bölümler tamamlandı!", 26);
            UIFactory.Button(card, "Tekrar oyna", session.Restart);
            UIFactory.Button(card, "Ana menü", SceneNavigator.Menu);
        }

        public void ShowError(string message)
        {
            BeginDialog("Bölüm yüklenemedi");
            UIFactory.Label(card, message, 22, 100f);
            UIFactory.Button(card, "Ana menü", SceneNavigator.Menu);
        }
    }
}
