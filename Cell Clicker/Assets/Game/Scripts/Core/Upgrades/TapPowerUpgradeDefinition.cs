using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>An Upgrade that boosts the DNA per tap by a share per level, added up (on top of Tap Power levels).</summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Cell Clicker/Upgrades/Tap Power")]
    public class TapPowerUpgradeDefinition : UpgradeDefinition
    {
        public override void ApplyTo(DnaModifiers modifiers, int level) => modifiers.MultiplyTapPower(Factor(level));

        public override string DescribeNextLevel() => "+" + Percent(AmountPerLevel) + " DNA per tap";

        public override string DescribeTotal(int level) => Times(Factor(level)) + " DNA per tap";

        private double Factor(int level) => 1.0 + AmountPerLevel * level;
    }
}
