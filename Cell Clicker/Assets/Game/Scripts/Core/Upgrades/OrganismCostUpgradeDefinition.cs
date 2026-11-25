using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// An Upgrade that makes every Organism Level cheaper. Each level takes its share off the current price
    /// (multiplied, not added), so prices can never reach zero however many levels exist.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Cell Clicker/Upgrades/Organism Cost")]
    public class OrganismCostUpgradeDefinition : UpgradeDefinition
    {
        public override void ApplyTo(DnaModifiers modifiers, int level) => modifiers.MultiplyOrganismCost(Factor(level));

        public override string DescribeNextLevel() => "-" + Percent(AmountPerLevel) + " Organism cost";

        public override string DescribeTotal(int level) => "-" + Percent(1.0 - Factor(level)) + " Organism cost";

        private double Factor(int level) => Math.Pow(1.0 - AmountPerLevel, level);

        protected override void OnValidate()
        {
            base.OnValidate();
            if (AmountPerLevel >= 1f) Debug.LogWarning($"{name}: a share of 100% or more would make Organisms free.", this);
        }
    }
}
