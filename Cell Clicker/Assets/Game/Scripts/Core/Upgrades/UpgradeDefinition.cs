using System;
using System.Globalization;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Design data for one Upgrade: name, look, Evolution Stage, price curve and how many levels it has.
    /// Each subclass is one kind of effect (all DNA, Tap Power, Organism cost...). New kind of effect = new
    /// subclass; new Upgrade = new asset, added to the Economy Config's list.
    /// </summary>
    public abstract class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Upgrade";
        [Tooltip("Stable name in save files. Never change it after release, or players lose this Upgrade's levels. Empty = the asset name.")]
        [SerializeField] private string saveId;
        [Tooltip("Optional. Empty = the code-drawn placeholder icon, tinted.")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color tint = Color.white;
        [Tooltip("The Evolution Stage this Upgrade belongs to (1 = Cell). For era-specific Upgrades once Evolution exists; today every stage is shown.")]
        [Min(1)]
        [SerializeField] private int evolutionStage = 1;

        [Header("Economy")]
        [Tooltip("DNA cost of the first level.")]
        [SerializeField] private double baseCost = 1000;
        [Tooltip("Each level costs this much more than the last (1.6 = +60%).")]
        [SerializeField] private float costGrowth = 1.6f;
        [Tooltip("How many times it can be bought.")]
        [Min(1)]
        [SerializeField] private int maxLevel = 10;
        [Tooltip("Effect of one level, as a share: 0.25 = 25%.")]
        [SerializeField] private float amountPerLevel = 0.25f;

        public string DisplayName => displayName;
        public string SaveId => string.IsNullOrEmpty(saveId) ? name : saveId;
        public Sprite Icon => icon;
        public Color Tint => tint;
        public int EvolutionStage => evolutionStage;
        public int MaxLevel => maxLevel;
        protected float AmountPerLevel => amountPerLevel;

        /// <summary>Price with <paramref name="level"/> levels already bought: base × growth^level, rounded (same as the economy sheet).</summary>
        public double CostAt(int level) => Math.Round(baseCost * Math.Pow(costGrowth, level));

        /// <summary>Multiplies this Upgrade's effect at <paramref name="level"/> into <paramref name="modifiers"/>.</summary>
        public abstract void ApplyTo(DnaModifiers modifiers, int level);

        /// <summary>What one more level does, e.g. "+25% all DNA".</summary>
        public abstract string DescribeNextLevel();

        /// <summary>The total effect at <paramref name="level"/>, e.g. "x1.75 all DNA".</summary>
        public abstract string DescribeTotal(int level);

        protected static string Percent(double share) =>
            Math.Round(share * 100).ToString("0", CultureInfo.InvariantCulture) + "%";

        protected static string Times(double factor) =>
            "x" + factor.ToString("0.##", CultureInfo.InvariantCulture);

        protected virtual void OnValidate()
        {
            baseCost = Math.Max(0, baseCost);
            costGrowth = Mathf.Max(1f, costGrowth);
            amountPerLevel = Mathf.Max(0f, amountPerLevel);
        }
    }
}
