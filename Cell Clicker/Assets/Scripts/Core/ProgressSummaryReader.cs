using System;
using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>Reads the <see cref="ProgressSummary"/> from the save without writing anything.</summary>
    public static class ProgressSummaryReader
    {
        /// <returns>False for a new player (no save yet) or a save without a summary.</returns>
        public static bool TryRead(ISaveService save, out ProgressSummaryData data, out DateTime savedAtUtc)
        {
            data = null;
            savedAtUtc = default;
            if (save?.LastSavedUtc == null) return false;

            string json = save.Load(ProgressSummary.Key);
            if (json == null) return false;
            savedAtUtc = save.LastSavedUtc.Value;

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
