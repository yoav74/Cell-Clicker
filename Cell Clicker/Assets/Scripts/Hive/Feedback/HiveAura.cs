using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.Hive
{
    /// <summary>
    /// Glow and spinning light rays behind the hive. Every tap adds "energy", so tapping fast
    /// makes the glow brighter and bigger and the rays spin faster. It rewards tapping streaks.
    /// </summary>
    public class HiveAura : HiveTapFeedback
    {
        [SerializeField] private Image glow;
        [SerializeField] private Image rays;

        [Header("Energy")]
        [SerializeField] private float energyPerTap = 0.1f;
        [SerializeField] private float energyDecayPerSecond = 0.6f;

        [Header("Glow")]
        [SerializeField] private float glowBaseAlpha = 0.35f;
        [SerializeField] private float glowMaxAlpha = 0.8f;
        [SerializeField] private float glowMaxExtraScale = 0.15f;
        [SerializeField] private float glowPulseSpeed = 1.2f;

        [Header("Rays")]
        [SerializeField] private float raysBaseAlpha = 0.1f;
        [SerializeField] private float raysMaxAlpha = 0.35f;
        [SerializeField] private float raysBaseSpin = 10f;  // degrees per second
        [SerializeField] private float raysMaxSpin = 120f;

        private float energy;
        private float flash;
        private Color glowColor;
        private Color raysColor;

        private void Awake()
        {
            glowColor = glow.color;
            raysColor = rays.color;
        }

        protected override void OnHiveTapped(HiveTap tap)
        {
            energy = Mathf.Min(1f, energy + energyPerTap);
            flash = 1f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            energy = Mathf.Max(0f, energy - energyDecayPerSecond * dt);
            flash = Mathf.Lerp(flash, 0f, 1f - Mathf.Exp(-10f * dt));

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * glowPulseSpeed * Mathf.PI * 2f);
            float glowAmount = Mathf.Clamp01(energy + flash * 0.3f);

            glowColor.a = Mathf.Lerp(glowBaseAlpha, glowMaxAlpha, glowAmount) * (0.85f + 0.15f * pulse);
            glow.color = glowColor;
            glow.rectTransform.localScale = Vector3.one * (1f + glowMaxExtraScale * glowAmount + 0.03f * pulse);

            raysColor.a = Mathf.Lerp(raysBaseAlpha, raysMaxAlpha, glowAmount);
            rays.color = raysColor;
            rays.rectTransform.Rotate(0f, 0f, -Mathf.Lerp(raysBaseSpin, raysMaxSpin, energy) * dt);
        }
    }
}
