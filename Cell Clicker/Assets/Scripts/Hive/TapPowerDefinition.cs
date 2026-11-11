using System;
using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>
    /// Design data for Tap Power: its shop look, price curve and DNA per tap at each level.
    /// One asset (Create > Cell Clicker > Tap Power), tuned in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "Tap Power", menuName = "Cell Clicker/Tap Power")]
    public class TapPowerDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Tap Power";
        [Tooltip("Optional. Empty = the code-drawn placeholder icon, tinted.")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color tint = new Color32(255, 196, 64, 255);

        [Header("Economy")]
        [Tooltip("DNA per tap before any Tap Power upgrade.")]
        [SerializeField] private double baseTapPower = 1;
        [Tooltip("DNA per tap added by every level.")]
        [SerializeField] private double tapPowerPerLevel = 0.2;
        [Tooltip("DNA cost of the first level. Below the first Organism so it's the player's first purchase.")]
        [SerializeField] private double baseCost = 10;
        [Tooltip("Each level costs this much more than the last (1.4 = +40%).")]
        [SerializeField] private float costGrowth = 1.4f;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color Tint => tint;

        /// <summary>DNA needed to go from <paramref name="level"/> to the next level.</summary>
        public double CostAt(int level) => Math.Floor(baseCost * Math.Pow(costGrowth, level));

        public double TapPowerAt(int level) => baseTapPower + tapPowerPerLevel * level;

        private void OnValidate()
        {
            baseTapPower = Math.Max(0, baseTapPower);
            tapPowerPerLevel = Math.Max(0, tapPowerPerLevel);
            baseCost = Math.Max(0, baseCost);
            costGrowth = Mathf.Max(1f, costGrowth);
        }
    }
}
