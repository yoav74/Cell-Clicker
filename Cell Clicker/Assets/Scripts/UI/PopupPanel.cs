using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// A bottom sheet that slides up over a dimmed screen. Closes from its close buttons,
    /// a tap on the dim backdrop, or Android's back button (reported as Escape by the Input System).
    /// The GameObject is inactive while closed, so a closed popup costs nothing.
    /// </summary>
    public class PopupPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform sheet;
        [SerializeField] private CanvasGroup backdrop;
        [Tooltip("Dim backdrop, X button...")]
        [SerializeField] private Button[] closeButtons;

        [SerializeField] private float openDuration = 0.32f;
        [SerializeField] private float closeDuration = 0.18f;
        [Tooltip("How far past the target the sheet overshoots when opening (0 = no bounce).")]
        [SerializeField] private float overshoot = 1.2f;

        private float progress;   // 0 = closed, 1 = open
        private bool opening;

        public bool IsOpen => gameObject.activeSelf && opening;

        private void Awake()
        {
            foreach (Button button in closeButtons)
            {
                if (button != null) button.onClick.AddListener(Close);
            }
        }

        public void Open()
        {
            if (IsOpen) return;

            opening = true;
            gameObject.SetActive(true);
            Apply();
        }

        public void Close() => opening = false;

        private void Update()
        {
            if (opening && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }

            // Unscaled time, so the popup still animates if the game is ever paused with timeScale = 0.
            float duration = opening ? openDuration : closeDuration;
            float step = duration > 0f ? Time.unscaledDeltaTime / duration : 1f;
            float previous = progress;
            progress = Mathf.Clamp01(progress + (opening ? step : -step));
            if (progress != previous) Apply(); // settled = no changes, so the popup canvas isn't rebuilt every frame

            if (!opening && progress <= 0f) gameObject.SetActive(false);
        }

        private void Apply()
        {
            float hiddenY = -(sheet.rect.height + 50f);
            float eased = opening ? EaseOutBack(progress) : progress * progress;
            sheet.anchoredPosition = new Vector2(sheet.anchoredPosition.x, Mathf.LerpUnclamped(hiddenY, 0f, eased));
            backdrop.alpha = progress;
        }

        private float EaseOutBack(float t)
        {
            float c = overshoot + 1f;
            float u = t - 1f;
            return 1f + c * u * u * u + overshoot * u * u;
        }
    }
}
