using System.Collections.Generic;
using CellClicker.Core;
using CellClicker.Hive;
using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>
    /// Shows a "+1"-style popup where the hive was tapped. Popups are pooled (reused instead of
    /// created and destroyed every tap) so fast tapping doesn't cause garbage-collection hitches on phones.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class FloatingTextSpawner : HiveTapFeedback
    {
        [SerializeField] private TMP_FontAsset font;
        [Tooltip("TMP material preset, e.g. one with a thick outline. Empty = the font's default material.")]
        [SerializeField] private Material material;
        [SerializeField] private float fontSize = 72f;
        [SerializeField] private Color[] colors =
        {
            new Color32(180, 255, 225, 255),
            new Color32(255, 225, 120, 255),
            new Color32(210, 180, 255, 255),
        };
        [SerializeField] private float randomSpreadX = 40f;
        [SerializeField] private int prewarmCount = 20;

        private RectTransform layer;
        private readonly Stack<FloatingText> pool = new Stack<FloatingText>();

        // Cached so tapping doesn't build a new string every time.
        private double lastAmount = double.NaN;
        private string lastLabel;

        private void Awake()
        {
            layer = (RectTransform)transform;
            if (font == null) font = TMP_Settings.defaultFontAsset;

            for (int i = 0; i < prewarmCount; i++)
            {
                FloatingText popup = Create();
                popup.gameObject.SetActive(false);
                pool.Push(popup);
            }
        }

        protected override void OnHiveTapped(HiveTap tap)
        {
            if (tap.Amount != lastAmount)
            {
                lastAmount = tap.Amount;
                lastLabel = "+" + NumberFormatter.FormatRate(tap.Amount);
            }

            Spawn(lastLabel, tap.ScreenPosition, tap.EventCamera);
        }

        public void Spawn(string message, Vector2 screenPosition, Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPosition, eventCamera, out Vector2 local);
            local.x += Random.Range(-randomSpreadX, randomSpreadX);

            FloatingText popup = pool.Count > 0 ? pool.Pop() : Create();
            popup.transform.SetAsLastSibling();
            popup.Play(message, local, colors[Random.Range(0, colors.Length)]);
        }

        internal void Release(FloatingText popup)
        {
            popup.gameObject.SetActive(false);
            pool.Push(popup);
        }

        private FloatingText Create()
        {
            var go = new GameObject("Popup", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(layer, false);
            rect.sizeDelta = new Vector2(400f, 120f);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = font;
            if (material != null) text.fontSharedMaterial = material;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;

            var popup = go.AddComponent<FloatingText>();
            popup.Init(this);
            return popup;
        }
    }
}
