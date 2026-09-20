using UnityEngine;

namespace RunnerGame
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        private GameSession session;
        private Bounds bounds;
        public void Initialize(GameSession owner, Bounds playArea) { session = owner; bounds = playArea; }
        private void Update()
        {
            if (session == null || session.State != SessionState.Playing) return;
            Vector3 p = transform.position;
            if (p.y < -3f || Mathf.Abs(p.x - bounds.center.x) > bounds.extents.x + 1f ||
                Mathf.Abs(p.z - bounds.center.z) > bounds.extents.z + 1f)
                session.Respawn();
        }
    }
}
