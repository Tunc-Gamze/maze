using System;
using UnityEngine;

namespace RunnerGame
{
    public enum SessionState { Loading, Playing, Paused, Won, Error }

    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private GameConfig config = null;
        public SessionState State { get; private set; } = SessionState.Loading;
        public int LevelIndex { get; private set; }
        public PlayerMotor Player => level == null ? null : level.Player;
        public bool HasNextLevel => LevelIndex + 1 < config.levels.Length;
        public ScoreService Score { get; } = new ScoreService();
        public ProgressionService Progress { get; private set; }
        public string SaveWarning { get; private set; } = "";
        private LevelManager level;
        private GameUI ui;

        private void Start()
        {
            Time.timeScale = 1f;
            LevelIndex = SceneNavigator.SelectedLevel;
            ui = gameObject.AddComponent<GameUI>();
            ui.Initialize(this);
            try
            {
                if (config == null) throw new InvalidOperationException("GameConfig reference is missing.");
                config.Validate();
                Progress = SaveService.Progress(config);
                LevelIndex = Mathf.Clamp(LevelIndex, 0, Progress.HighestUnlocked);
                level = gameObject.AddComponent<LevelManager>();
                level.Load(config, LevelIndex, this);
                Player.Controls.Sensitivity = SaveService.Sensitivity;
                Score.Reset(level.Layout.Coins.Count);
                ui.RefreshHUD();
                State = SessionState.Playing;
            }
            catch (Exception error)
            {
                State = SessionState.Error;
                if (Player != null) Player.SetMovementEnabled(false);
                Debug.LogException(error, this);
                ui.ShowError(error.Message);
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (State == SessionState.Playing) Pause();
            else if (State == SessionState.Paused) Resume();
        }

        public void Pause()
        {
            if (State != SessionState.Playing) return;
            State = SessionState.Paused;
            Player.SetMovementEnabled(false);
            Time.timeScale = 0f;
            ui.ShowPause();
        }

        public void Resume()
        {
            if (State != SessionState.Paused) return;
            State = SessionState.Playing;
            Time.timeScale = 1f;
            Player.SetMovementEnabled(true);
            ui.HideDialog();
        }

        public void ReachGoal(PlayerMotor player)
        {
            if (State != SessionState.Playing || player != Player) return;
            State = SessionState.Won;
            Player.SetMovementEnabled(false);
            Time.timeScale = 0f;
            try { Progress.Complete(LevelIndex, Score.Score); SaveWarning = ""; }
            catch (Exception error)
            {
                SaveWarning = "İlerleme kaydedilemedi. Depolamayı kontrol edin.";
                Debug.LogException(error, this);
            }
            ui.ShowWin();
        }

        public void Respawn()
        {
            if (State != SessionState.Playing) return;
            Player.Stop();
            Player.Body.position = level.Spawn;
            Player.Body.rotation = Quaternion.identity;
            Player.Controls.Calibrate();
        }

        public bool TryCollectCoin(PlayerMotor player, int id)
        {
            if (State != SessionState.Playing || player != Player || !Score.TryCollect(id)) return false;
            ui.RefreshHUD();
            return true;
        }

        public void Restart() => SceneNavigator.Play(LevelIndex);
        public void NextLevel()
        {
            if (State == SessionState.Won && HasNextLevel && Progress.IsUnlocked(LevelIndex + 1))
                SceneNavigator.Play(LevelIndex + 1);
        }

        public void CycleSensitivity()
        {
            float current = Player.Controls.Sensitivity;
            Player.Controls.Sensitivity = current >= 3.5f ? 1.5f : current + 0.5f;
            try { SaveService.SetSensitivity(Player.Controls.Sensitivity); SaveWarning = ""; }
            catch (Exception error)
            {
                SaveWarning = "Hassasiyet kaydedilemedi.";
                Debug.LogException(error, this);
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Pause(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        private void OnDestroy() { Time.timeScale = 1f; }
    }
}
