using System;

namespace CellClicker.Core
{
    /// <summary>Anything that produces DNA/sec on its own (Organisms now; more sources later).</summary>
    public interface IDnaSource
    {
        double DnaPerSecond { get; }

        /// <summary>Fired whenever <see cref="DnaPerSecond"/> changes.</summary>
        event Action RateChanged;
    }
}
