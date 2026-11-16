using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Pays Offline Earnings into the <see cref="DnaBank"/>: once when the Level loads, and again whenever
    /// the app comes back from the background (Unity doesn't run Update while the app is paused).
    /// </summary>
    public class OfflineEarningsCollector : MonoBehaviour
    {
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private DnaGenerator generator;
        [SerializeField] private DnaBank bank;
        [SerializeField] private OfflineEarningsSettings settings;

        private DateTime? lastPaidUtc;
        private bool started;

        /// <summary>Fired with the DNA paid and the away time it was paid for. For a popup or VFX later.</summary>
        public event Action<double, TimeSpan> Collected;

        // SaveSystem (execution order -100) has loaded, and the Organisms restored their DNA/sec, before this Start.
        private void Start()
        {
            started = true;
            Collect();
        }

        // Unity may send OnApplicationPause(false) at launch, before Start and before the save is loaded: ignore that one.
        private void OnApplicationPause(bool paused)
        {
            if (!paused && started) Collect();
        }

        private void Collect()
        {
            DateTime now = DateTime.UtcNow;
            DateTime? since = Latest(saveSystem.LastSavedUtc, lastPaidUtc);
            lastPaidUtc = now;
            if (since == null) return; // new player: nothing to pay

            TimeSpan away = now - since.Value;
            double earned = settings.Calculate(generator.DnaPerSecond, away);
            if (earned <= 0) return;

            bank.Add(earned);
            Collected?.Invoke(earned, settings.PaidTime(away));
        }

        // Never pay the same stretch of time twice, even if no save happened in between.
        private static DateTime? Latest(DateTime? a, DateTime? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return a > b ? a : b;
        }
    }
}
