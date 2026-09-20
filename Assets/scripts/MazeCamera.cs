using UnityEngine;

namespace RunnerGame
{
    [RequireComponent(typeof(Camera))]
    public sealed class MazeCamera : MonoBehaviour
    {
        private Camera view;
        private Bounds bounds;
        private bool configured;
        private float lastAspect;

        public void Configure(Bounds levelBounds)
        {
            view = GetComponent<Camera>();
            bounds = levelBounds;
            configured = true;
            view.orthographic = true;
            view.nearClipPlane = 0.1f;
            view.farClipPlane = 100f;
            transform.rotation = Quaternion.Euler(65f, 0f, 0f);
            transform.position = bounds.center - transform.forward * 30f;
            Fit();
        }

        private void Start()
        {
            if (configured) return;
            GameObject ground = GameObject.FindWithTag("Ground");
            if (ground != null && ground.TryGetComponent<Collider>(out var floor))
                Configure(new Bounds(floor.bounds.center, floor.bounds.size + Vector3.up * 2f));
        }

        private void LateUpdate()
        {
            if (configured && !Mathf.Approximately(lastAspect, view.aspect)) Fit();
        }

        private void Fit()
        {
            float height = 0f;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                Vector3 local = transform.InverseTransformPoint(corner);
                height = Mathf.Max(height, Mathf.Abs(local.y), Mathf.Abs(local.x) / Mathf.Max(0.1f, view.aspect));
            }
            view.orthographicSize = Mathf.Max(2f, height * 1.3f);
            lastAspect = view.aspect;
        }
    }
}
