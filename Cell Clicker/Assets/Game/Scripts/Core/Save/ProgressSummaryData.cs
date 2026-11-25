using System;

namespace CellClicker.Core
{
    /// <summary>The few numbers the Home screen shows, saved by <see cref="ProgressSummary"/>.</summary>
    [Serializable]
    public class ProgressSummaryData
    {
        public double dna;
        public double dnaPerSecond;
        /// <summary>DNA earned during the last visit to the Level: the reward Home shows after playing.</summary>
        public double lastVisitEarned;
    }
}
