using UnityEngine;

namespace RunnerGame
{
    public sealed class GoalTrigger : MonoBehaviour
    {
        private GameSession session;
        public void Initialize(GameSession owner) => session = owner;
        private void OnTriggerEnter(Collider other)
        {
            if (session != null && other.attachedRigidbody != null &&
                other.attachedRigidbody.TryGetComponent<PlayerMotor>(out var player))
                session.ReachGoal(player);
        }
    }
}
