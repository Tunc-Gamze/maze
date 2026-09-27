using UnityEngine;

namespace RunnerGame
{
    public sealed class Coin : MonoBehaviour
    {
        private bool collected;
        private GameSession session;
        private int id;
        private Renderer visual;

        private void Awake() => visual = GetComponent<Renderer>();

        public void Initialize(GameSession owner, int coinId) { session = owner; id = coinId; }

        private void Update()
        {
            if (session == null || session.CoinTimeRunning)
                transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        }

        public void SetLifetime(float remaining, float warningSeconds)
        {
            // Visual only: collider remains collectable until the actual expiry event.
            if (visual != null) visual.enabled = remaining > warningSeconds || (int)(remaining * 5f) % 2 == 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (collected || body == null || !body.TryGetComponent<PlayerMotor>(out var player)) return;
            // Legacy test scenes have no session; their coin still disappears on contact.
            if (session != null)
            {
                if (session.TryCollectCoin(player, id)) collected = true;
                return; // The spawner owns session coin destruction and expiration.
            }
            collected = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnTriggerStay(Collider other) => OnTriggerEnter(other);
    }
}
