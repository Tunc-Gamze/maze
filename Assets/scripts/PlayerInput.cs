using UnityEngine;

namespace RunnerGame
{
    [DisallowMultipleComponent]
    public sealed class PlayerInput : MonoBehaviour
    {
        [Range(0.02f, 0.3f)] [SerializeField] private float deadZone = 0.07f;
        [Range(0.5f, 5f)] public float Sensitivity = 2.5f;
        [Min(0.01f)] [SerializeField] private float smoothingSeconds = 0.12f;
        [Min(0.1f)] [SerializeField] private float calibrationSeconds = 0.75f;
        public Vector2 Movement { get; private set; }
        public bool IsCalibrating { get; private set; }
        public bool UsesTilt => Application.isMobilePlatform && !Application.isEditor && SystemInfo.supportsAccelerometer;
        public bool SensorUnavailable => Application.isMobilePlatform && !Application.isEditor && !SystemInfo.supportsAccelerometer;
        private Vector2 neutral;
        private Vector2 samples;
        private float sampleTime;
        private int sampleCount;
        private float calibrationWarmup;
        private bool accepting = true;

        private void Awake()
        {
            // Unity compensates accelerometer XY for the fixed landscape screen.
            // Do not rotate the same sample again in application code.
            Input.compensateSensors = true;
            if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;
            Calibrate();
        }

        public void SetAccepting(bool value)
        {
            accepting = value;
            Movement = Vector2.zero;
        }

        public void Calibrate()
        {
            Movement = Vector2.zero;
            samples = Vector2.zero;
            sampleTime = 0f;
            sampleCount = 0;
            calibrationWarmup = 0.2f;
            IsCalibrating = UsesTilt;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) Calibrate();
            if (IsCalibrating)
            {
                // Let a newly requested screen orientation settle before sampling.
                if (calibrationWarmup > 0f)
                {
                    calibrationWarmup -= Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                    Movement = Vector2.zero;
                    return;
                }
                Vector3 acceleration = Input.acceleration;
                samples += new Vector2(acceleration.x, acceleration.y);
                sampleCount++;
                sampleTime += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                if (sampleTime >= calibrationSeconds && sampleCount >= 10)
                {
                    neutral = samples / Mathf.Max(1, sampleCount);
                    IsCalibrating = false;
                }
                Movement = Vector2.zero;
                return;
            }
            if (!accepting) { Movement = Vector2.zero; return; }

            Vector2 desired;
            if (UsesTilt)
            {
                Vector3 acceleration = Input.acceleration;
                Vector2 tilt = new Vector2(acceleration.x, acceleration.y) - neutral;
                float magnitude = tilt.magnitude;
                desired = magnitude <= deadZone ? Vector2.zero : tilt.normalized *
                    Mathf.Clamp01((magnitude - deadZone) * Sensitivity);
            }
            else
                desired = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

            float blend = 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.01f, smoothingSeconds));
            Movement = Vector2.Lerp(Movement, desired, blend);
            if (desired == Vector2.zero && Movement.sqrMagnitude < 0.0001f) Movement = Vector2.zero;
        }
    }
}
