using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RunnerGame
{
    // Keyboard/gamepad navigation should not select a level hidden below the viewport.
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollSelectionIntoView : MonoBehaviour
    {
        private ScrollRect scroll;
        private GameObject lastSelection;
        private void Awake() => scroll = GetComponent<ScrollRect>();

        private void LateUpdate()
        {
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (selected == lastSelection) return;
            lastSelection = selected;
            if (selected == null || scroll.content == null || !selected.transform.IsChildOf(scroll.content)) return;
            Canvas.ForceUpdateCanvases();
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, selected.transform);
            Rect view = scroll.viewport.rect;
            float shift = bounds.min.y < view.yMin ? view.yMin - bounds.min.y :
                bounds.max.y > view.yMax ? view.yMax - bounds.max.y : 0f;
            if (Mathf.Approximately(shift, 0f)) return;
            scroll.StopMovement();
            Vector2 position = scroll.content.anchoredPosition;
            position.y = Mathf.Clamp(position.y + shift, 0f, Mathf.Max(0f, scroll.content.rect.height - view.height));
            scroll.content.anchoredPosition = position;
        }
    }
}
