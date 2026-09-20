using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RunnerGame
{
    public static class UIFactory
    {
        public static readonly Color Ink = new Color(0.07f, 0.11f, 0.18f, 0.97f);
        public static readonly Color Accent = new Color(0.08f, 0.48f, 0.55f);

        public static RectTransform Canvas(string name)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 1f;
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            RectTransform safe = Rect("Safe Area", root.transform);
            safe.gameObject.AddComponent<SafeArea>();
            return safe;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Anchor(rect, Vector2.zero, Vector2.one);
            return rect;
        }

        public static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static RectTransform Panel(string name, Transform parent, Color color)
        {
            RectTransform rect = Rect(name, parent);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        public static void Vertical(RectTransform rect, float spacing = 12f)
        {
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        public static Text Label(Transform parent, string value, int fontSize = 28, float height = 46f)
        {
            RectTransform rect = Rect("Label", parent);
            Text label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.text = value;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = fontSize;
            label.raycastTarget = false;
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = layout.preferredHeight = height;
            return label;
        }

        public static Button Button(Transform parent, string title, UnityAction action)
        {
            RectTransform rect = Panel(title, parent, Accent);
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = layout.preferredHeight = 52f;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
            button.colors = colors;
            button.onClick.AddListener(action);
            Label(rect, title, 25);
            return button;
        }

        public static RectTransform ScrollList(Transform parent)
        {
            RectTransform viewport = Panel("Level List", parent, new Color(0.03f, 0.06f, 0.1f, 0.7f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var size = viewport.gameObject.AddComponent<LayoutElement>();
            size.minHeight = 100f;
            size.preferredHeight = 240f;
            size.flexibleHeight = 1f;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            RectTransform content = Rect("Levels", viewport);
            Anchor(content, new Vector2(0f, 1f), Vector2.one);
            content.pivot = new Vector2(0.5f, 1f);
            Vertical(content, 8f);
            content.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            content.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(8, 8, 8, 8);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            viewport.gameObject.AddComponent<ScrollSelectionIntoView>();
            return content;
        }

        public static void Clear(Transform parent)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}
