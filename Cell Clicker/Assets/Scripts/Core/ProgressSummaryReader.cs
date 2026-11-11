using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>Reads the <see cref="ProgressSummary"/> from the save file without writing anything.</summary>
    public static class ProgressSummaryReader
    {
        /// <returns>False for a new player (no save yet) or a save without a summary.</returns>
        public static bool TryRead(out ProgressSummaryData data, out DateTime savedAtUtc)
        {
            data = null;
            savedAtUtc = default;

            if (!new SaveFileStore(SaveSystem.DefaultPath).TryLoad(out SaveFile file)) return false;

            string json = file.Find(ProgressSummary.Key);
            if (json == null || !SaveSystem.TryGetUtc(file.savedAtUtcTicks, out savedAtUtc)) return false;

            try
            {
                data = JsonUtility.FromJson<ProgressSummaryData>(json);
            }
            catch (ArgumentException)
            {
                return false; // broken JSON: treat as no summary
            }
            return data != null;
        }
    }
}
