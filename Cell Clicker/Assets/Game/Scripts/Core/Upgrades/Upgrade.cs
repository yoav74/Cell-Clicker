using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>The player's copy of one Upgrade: which definition it is and how many levels are bought.</summary>
    public class Upgrade
    {
        public Upgrade(UpgradeDefinition definition)
        {
            Definition = definition;
        }

        public UpgradeDefinition Definition { get; }

        /// <summary>0 = never bought.</summary>
        public int Level { get; private set; }

        public bool IsMaxed => Level >= Definition.MaxLevel;
        public double NextCost => Definition.CostAt(Level);

        internal void LevelUp() => Level++;

        /// <summary>Only for loading a save. Clamped, so lowering the max later can't leave a save above it.</summary>
        internal void SetLevel(int level) => Level = Mathf.Clamp(level, 0, Definition.MaxLevel);
    }
}
