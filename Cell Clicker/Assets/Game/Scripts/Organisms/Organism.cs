using System;

namespace CellClicker.Organisms
{
    /// <summary>The player's copy of one Organism: which definition it is and its Organism Level.</summary>
    public class Organism
    {
        public Organism(OrganismDefinition definition)
        {
            Definition = definition;
        }

        public OrganismDefinition Definition { get; }

        /// <summary>0 = not recruited yet.</summary>
        public int Level { get; private set; }

        public bool IsRecruited => Level > 0;
        public double NextCost => Definition.CostAt(Level);
        public double DnaPerSecond => Definition.DnaPerSecondAt(Level);
        public double NextLevelGain => Definition.DnaPerSecondAt(Level + 1) - DnaPerSecond;

        internal void LevelUp() => Level++;

        /// <summary>Only for loading a save.</summary>
        internal void SetLevel(int level) => Level = Math.Max(0, level);
    }
}
