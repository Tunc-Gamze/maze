using UnityEngine;

namespace RunnerGame
{
    public sealed class Coin : MonoBehaviour
    {
        private bool collected;
        private GameSession session;
        private int id;

        public void Initialize(GameSession owner, int coinId) { session = owner; id = coinId; }

        private void Update() => transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (collected || body == null || !body.TryGetComponent<PlayerMotor>(out var player)) return;
            // Legacy test scenes have no session; their coin still disappears on contact.
            if (session != null && !session.TryCollectCoin(player, id)) return;
            collected = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
