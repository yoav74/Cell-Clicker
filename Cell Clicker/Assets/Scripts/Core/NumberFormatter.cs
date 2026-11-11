using System;
using System.Globalization;

namespace CellClicker.Core
{
    /// <summary>Turns big numbers into short clicker-style text: 1234 -> "1.23K".</summary>
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string Format(double value)
        {
            if (value < 1000) return Math.Floor(value).ToString("0", CultureInfo.InvariantCulture);

            int tier = Math.Min((int)(Math.Log10(value) / 3), Suffixes.Length - 1);
            double scaled = value / Math.Pow(1000, tier);

            // Round down so the number never shows more DNA than the player has.
            int decimals = scaled < 10 ? 2 : scaled < 100 ? 1 : 0;
            double factor = Math.Pow(10, decimals);
            scaled = Math.Floor(scaled * factor) / factor;

            return scaled.ToString("0.##", CultureInfo.InvariantCulture) + Suffixes[tier];
        }

        /// <summary>Like <see cref="Format"/>, but keeps one decimal below 100 so small DNA/sec rates (e.g. 0.1) don't show as 0.</summary>
        public static string FormatRate(double value)
        {
            if (value >= 100) return Format(value);

            double floored = Math.Floor(value * 10) / 10;
            return floored.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
