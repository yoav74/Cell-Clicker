using CellClicker.Platform;
using CellClicker.UI;
using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>Plays a short blip on every hive tap, with a slightly different pitch each time.</summary>
    public class HiveTapSound : HiveTapFeedback
    {
        [Tooltip("Leave empty to use the placeholder blip made in code.")]
        [SerializeField] private AudioClip clip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.6f;
        [Tooltip("Random pitch per tap, so fast tapping doesn't sound robotic.")]
        [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.12f);

        private AudioClip sound;

        // Build the placeholder now, so the first tap doesn't pay for generating it.
        private void Awake() => sound = clip != null ? clip : ProceduralSounds.Get(ProceduralSoundKind.Tap);

        protected override void OnHiveTapped(HiveTap tap) =>
            Services.Sound?.Play(sound, volume, Random.Range(pitchRange.x, pitchRange.y));
    }
}
