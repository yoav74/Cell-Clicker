using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Saves a small summary (DNA total, DNA/sec) next to the full save, so the Home screen can show
    /// progress and Offline Earnings without loading the Level's systems or knowing their save formats.
    /// </summary>
    public class ProgressSummary : MonoBehaviour, ISaveable
    {
        public const string Key = "summary";

        [SerializeField] private DnaBank bank;
        [SerializeField] private DnaGenerator generator;

        public string SaveKey => Key;

        public string CaptureState() => JsonUtility.ToJson(new ProgressSummaryData
        {
            dna = bank.Amount,
            dnaPerSecond = generator.DnaPerSecond,
        });

        // Nothing to restore: the summary is rebuilt from the bank and the generator on every save.
        public void RestoreState(string json) { }
    }
}
