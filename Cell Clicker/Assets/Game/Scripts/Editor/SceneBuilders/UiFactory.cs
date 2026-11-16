using CellClicker.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>Small helpers the scene builders share to create and wire uGUI objects.</summary>
    public static class UiFactory
    {
        // The Pill placeholder sprite has 32 px corners; Image.pixelsPerUnitMultiplier rescales them.
        private const float PillCornerPixels = 32f;

        // Course minimum for a thumb: 140 x 140 at the 1080 x 1920 reference.
        public const float MinTapSize = 140f;

        /// <summary>
        /// Overlay canvas, portrait 1080x1920 reference. Matches width (0) instead of the course's 0.5:
        /// on tall phones 0.5 would shrink the usable width below 1080 and clip the header and shop rows.
        /// </summary>
        public static GameObject CreatePortraitCanvas(string name)
        {
            var canvasGo = new GameObject(name, typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            return canvasGo;
        }

        /// <summary>A round, 140x140 button with a text glyph, pinned to <paramref name="anchor"/> of its parent.</summary>
        public static UnityEngine.UI.Button BuildIconButton(Transform parent, string name, string glyph, Vector2 anchor, Vector2 position)
        {
            RectTransform rect = Pin(Create(name, parent), anchor, new Vector2(MinTapSize, MinTapSize), position);
            UnityEngine.UI.Image image = AddRounded(rect.gameObject, new Color(1f, 1f, 1f, 0.12f), MinTapSize * 0.5f, raycast: true);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            RectTransform mark = Create("Glyph", rect);
            Stretch(mark);
            AddText(mark.gameObject, glyph, 64, Color.white, TmpSetup.PopupPreset);
            return button;
        }

        public static RectTransform Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Fixed-size rect pinned to one point of its parent; the pivot sits on that point too.</summary>
        public static RectTransform Pin(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        /// <summary>Strip along the top of its parent: <paramref name="top"/> down from the top edge, inset left and right.</summary>
        public static RectTransform PinTop(RectTransform rect, float top, float height, float left = 0f, float right = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static UnityEngine.UI.Image AddImage(GameObject go, Color color, bool raycast = false)
        {
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        /// <summary>An Image showing a code-drawn placeholder sprite.</summary>
        public static UnityEngine.UI.Image AddPlaceholder(GameObject go, ProceduralSpriteKind kind, Color color, bool raycast = false)
        {
            UnityEngine.UI.Image image = AddImage(go, color, raycast);
            var placeholder = go.AddComponent<ProceduralImage>();
            var so = new SerializedObject(placeholder);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.ApplyModifiedPropertiesWithoutUndo();
            placeholder.enabled = false; // re-enable so the sprite shows right away in the editor
            placeholder.enabled = true;
            return image;
        }

        /// <summary>A rounded rectangle (9-sliced Pill) with the given corner radius in canvas units.</summary>
        public static UnityEngine.UI.Image AddRounded(GameObject go, Color color, float radius, bool raycast = false)
        {
            UnityEngine.UI.Image image = AddPlaceholder(go, ProceduralSpriteKind.Pill, color, raycast);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PillCornerPixels / Mathf.Max(1f, radius);
            return image;
        }

        public static TMP_Text AddText(GameObject go, string content, float size, Color color, Material preset,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = TmpSetup.DefaultFont;
            if (preset != null) text.fontSharedMaterial = preset;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        public static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void WireArray(Object target, string field, params Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty array = so.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
