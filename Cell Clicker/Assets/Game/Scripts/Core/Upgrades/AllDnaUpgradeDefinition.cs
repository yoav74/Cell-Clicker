using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>An Upgrade that boosts all DNA earned (taps and DNA/sec) by a share per level, added up.</summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Cell Clicker/Upgrades/All DNA")]
    public class AllDnaUpgradeDefinition : UpgradeDefinition
    {
        public override void ApplyTo(DnaModifiers modifiers, int level) => modifiers.MultiplyAllDna(Factor(level));

        public override string DescribeNextLevel() => "+" + Percent(AmountPerLevel) + " all DNA";

        public override string DescribeTotal(int level) => Times(Factor(level)) + " all DNA";

        private double Factor(int level) => 1.0 + AmountPerLevel * level;
    }
}
