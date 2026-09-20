using UnityEngine;

namespace RunnerGame
{
    public sealed class SafeArea : MonoBehaviour
    {
        private Rect last;
        private Vector2 size;
        private void Update()
        {
            Rect safe = Screen.safeArea;
            if (safe == last && size == new Vector2(Screen.width, Screen.height)) return;
            last = safe;
            size = new Vector2(Screen.width, Screen.height);
            if (size.x <= 0 || size.y <= 0) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / size.x, safe.yMin / size.y);
            rect.anchorMax = new Vector2(safe.xMax / size.x, safe.yMax / size.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
