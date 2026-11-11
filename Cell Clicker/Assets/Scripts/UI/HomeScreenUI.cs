using System;
using CellClicker.Core;
using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>
    /// Home screen: shows the saved DNA total and, after a break, a "While you were away" card with the
    /// Offline Earnings waiting in the Level. The DNA itself is added by <see cref="OfflineEarningsCollector"/>.
    /// </summary>
    public class HomeScreenUI : MonoBehaviour
    {
        [SerializeField] private OfflineEarningsSettings offlineSettings;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text rateText;
        [SerializeField] private GameObject awayCard;
        [SerializeField] private TMP_Text awayAmountText;
        [SerializeField] private TMP_Text awayDetailText;

        private void Start()
        {
            awayCard.SetActive(false);

            if (!ProgressSummaryReader.TryRead(out ProgressSummaryData summary, out DateTime savedAtUtc))
            {
                totalText.text = "0";
                rateText.text = "Tap the DNA Hive to start";
                return;
            }

            TimeSpan away = DateTime.UtcNow - savedAtUtc;
            double earned = offlineSettings.Calculate(summary.dnaPerSecond, away);

            // Include the waiting DNA so the number on Home matches what the Level shows.
            totalText.text = NumberFormatter.Format(summary.dna + earned);
            rateText.text = NumberFormatter.FormatRate(summary.dnaPerSecond) + " DNA/sec";
            if (earned <= 0) return;

            awayCard.SetActive(true);
            awayAmountText.text = "+" + NumberFormatter.Format(earned) + " DNA";
            awayDetailText.text = "in " + FormatDuration(offlineSettings.PaidTime(away)) + " away";
        }

        private static string FormatDuration(TimeSpan time)
        {
            if (time.TotalHours >= 1) return $"{(int)time.TotalHours}h {time.Minutes}m";
            return $"{Math.Max(1, time.Minutes)}m";
        }
    }
}
