using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>A short vibration on every hive tap, so the tap is felt as well as seen.</summary>
    public class HiveTapHaptics : HiveTapFeedback
    {
        [Tooltip("Keep it Light: players tap many times a second.")]
        [SerializeField] private HapticStrength strength = HapticStrength.Light;

        protected override void OnHiveTapped(HiveTap tap) => Services.Haptics?.Pulse(strength);
    }
}
