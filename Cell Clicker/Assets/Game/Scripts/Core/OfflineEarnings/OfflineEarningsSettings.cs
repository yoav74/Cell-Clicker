using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Tuning for Offline Earnings (DNA earned while the game was closed). One asset shared by the
    /// Home screen preview and the Level, so both always show the same number.
    /// </summary>
    [CreateAssetMenu(fileName = "Offline Earnings", menuName = "Cell Clicker/Offline Earnings Settings")]
    public class OfflineEarningsSettings : ScriptableObject
    {
        [Tooltip("Share of DNA/sec earned while away (0.25 = 25%). Keeps active play worth more than idling.")]
        [Range(0f, 1f)]
        [SerializeField] private float rateShare = 0.25f;
        [Tooltip("Away time beyond this is not paid, so players still have a reason to come back.")]
        [SerializeField] private float maxHours = 2f;
        [Tooltip("Shorter breaks (e.g. switching scenes) pay nothing and show no card.")]
        [SerializeField] private float minAwaySeconds = 60f;

        public float RateShare => rateShare;
        public TimeSpan MaxAway => TimeSpan.FromHours(maxHours);

        /// <summary>The away time that counts: zero below the minimum (or if the clock went backwards), capped at the max.</summary>
        public TimeSpan PaidTime(TimeSpan away)
        {
            if (away.TotalSeconds < minAwaySeconds) return TimeSpan.Zero;
            return away > MaxAway ? MaxAway : away;
        }

        public double Calculate(double dnaPerSecond, TimeSpan away)
        {
            if (!double.IsFinite(dnaPerSecond) || dnaPerSecond <= 0) return 0;

            double earned = dnaPerSecond * rateShare * PaidTime(away).TotalSeconds;
            return double.IsFinite(earned) ? earned : 0;
        }

        private void OnValidate()
        {
            maxHours = Mathf.Max(0f, maxHours);
            minAwaySeconds = Mathf.Max(0f, minAwaySeconds);
        }
    }
}
