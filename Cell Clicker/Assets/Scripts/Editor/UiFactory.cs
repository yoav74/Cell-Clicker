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
