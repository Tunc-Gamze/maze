using UnityEngine;

namespace RunnerGame
{
    // Visual-only rotation. Replacing this child must never replace the physics root.
    public sealed class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private float turnSpeed = 540f;
        private Rigidbody body;

        private void Awake() => body = GetComponentInParent<Rigidbody>();

        private void LateUpdate()
        {
            if (body == null) return;
            Vector3 direction = body.velocity;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }
    }
}
