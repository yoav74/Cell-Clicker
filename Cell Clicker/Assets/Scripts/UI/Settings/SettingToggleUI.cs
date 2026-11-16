using CellClicker.Platform;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// A button that flips one on/off setting in <see cref="Services.Settings"/> and shows its state.
    /// Subclasses say which setting; the look and the click feedback are shared here.
    /// </summary>
    public abstract class SettingToggleUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;

        [Header("Look")]
        [SerializeField] private string title = "Setting";
        [SerializeField] private Color onColor = new Color32(76, 196, 120, 255);
        [SerializeField] private Color offColor = new Color32(58, 74, 86, 255);
        [Tooltip("Leave empty to use the placeholder click made in code.")]
        [SerializeField] private AudioClip clickSound;

        protected static ISettingsService Settings => Services.Settings;

        protected abstract bool Value { get; set; }

        private void OnEnable()
        {
            // Without Bootstrap there are no settings to change.
            button.interactable = Settings != null;
            if (Settings == null) return;

            Settings.Changed += Refresh;
            button.onClick.AddListener(Toggle);
            Refresh();
        }

        private void OnDisable()
        {
            if (Settings == null) return;

            Settings.Changed -= Refresh;
            button.onClick.RemoveListener(Toggle);
        }

        // Feedback after the change, so turning Sound on plays the click and turning Haptics on buzzes.
        private void Toggle()
        {
            Value = !Value;
            Services.Sound?.Play(clickSound != null ? clickSound : ProceduralSounds.Get(ProceduralSoundKind.Click));
            Services.Haptics?.Pulse(HapticStrength.Light);
        }

        private void Refresh()
        {
            label.text = Value ? title + "\nON" : title + "\nOFF";
            background.color = Value ? onColor : offColor;
        }
    }
}
