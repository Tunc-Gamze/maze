using UnityEngine;

namespace RunnerGame
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    [DisallowMultipleComponent]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Min(0.1f)] [SerializeField] private float maxSpeed = 2.8f;
        [Min(0.1f)] [SerializeField] private float acceleration = 14f;
        [Min(0.1f)] [SerializeField] private float braking = 20f;
        private Rigidbody body;
        private PlayerInput controls;
        private bool canMove = true;
        public Rigidbody Body => body;
        public PlayerInput Controls => controls;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            controls = GetComponent<PlayerInput>();
        }

        public void SetMovementEnabled(bool value)
        {
            canMove = value;
            controls.SetAccepting(value);
            Stop();
        }

        public void Stop()
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (!canMove || controls.IsCalibrating) { Stop(); return; }
            Vector2 input = controls.Movement;
            Vector3 target = new Vector3(input.x, 0f, input.y) * maxSpeed;
            Vector3 horizontal = new Vector3(body.velocity.x, 0f, body.velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, target,
                (input.sqrMagnitude < 0.001f ? braking : acceleration) * Time.fixedDeltaTime);
            horizontal = Vector3.ClampMagnitude(horizontal, maxSpeed);
            body.velocity = new Vector3(horizontal.x, body.velocity.y, horizontal.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Legacy scenes keep a goal signal until the session system is installed.
            if (other.CompareTag("Finish")) Debug.Log("Goal reached.", this);
        }
    }
}
