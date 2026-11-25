using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Saves a small summary (DNA total, DNA/sec, DNA earned this visit) next to the full save, so the Home
    /// screen can show progress, the last visit's reward and Offline Earnings without loading the Level's
    /// systems or knowing their save formats.
    /// </summary>
    public class ProgressSummary : MonoBehaviour, ISaveable
    {
        public const string Key = "summary";

        [SerializeField] private DnaBank bank;
        [SerializeField] private DnaGenerator generator;

        // Counts from zero every time the Level opens, so it is this visit's reward only.
        private double earnedThisVisit;

        public string SaveKey => Key;

        private void OnEnable() => bank.Earned += OnEarned;

        private void OnDisable() => bank.Earned -= OnEarned;

        private void OnEarned(double amount) => earnedThisVisit += amount;

        public string CaptureState() => JsonUtility.ToJson(new ProgressSummaryData
        {
            dna = bank.Amount,
            dnaPerSecond = generator.DnaPerSecond,
            lastVisitEarned = earnedThisVisit,
        });

        // Nothing to restore: the summary is rebuilt from the bank and the generator on every save.
        public void RestoreState(string json) { }
    }
}
