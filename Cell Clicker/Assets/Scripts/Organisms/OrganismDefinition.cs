using System;
using UnityEngine;

namespace CellClicker.Organisms
{
    /// <summary>
    /// Design data for one Organism: name, look, price curve and DNA/sec.
    /// New Organism = new asset (Create > Cell Clicker > Organism), no code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "Organism", menuName = "Cell Clicker/Organism")]
    public class OrganismDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Organism";
        [Tooltip("Stable name in save files. Never change it after release, or players lose this Organism's levels. Empty = the asset name.")]
        [SerializeField] private string saveId;
        [Tooltip("Optional. Empty = the code-drawn placeholder icon, tinted.")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color tint = Color.white;

        [Header("Economy")]
        [Tooltip("DNA cost to recruit (go from Organism Level 0 to 1).")]
        [SerializeField] private double baseCost = 15;
        [Tooltip("Each Organism Level costs this much more than the last (1.15 = +15%).")]
        [SerializeField] private float costGrowth = 1.15f;
        [Tooltip("DNA/sec added by every Organism Level.")]
        [SerializeField] private double dnaPerSecondPerLevel = 0.1;

        public string DisplayName => displayName;
        public string SaveId => string.IsNullOrEmpty(saveId) ? name : saveId;
        public Sprite Icon => icon;
        public Color Tint => tint;

        /// <summary>DNA needed to go from <paramref name="level"/> to the next Organism Level.</summary>
        public double CostAt(int level) => Math.Floor(baseCost * Math.Pow(costGrowth, level));

        public double DnaPerSecondAt(int level) => dnaPerSecondPerLevel * level;

        private void OnValidate()
        {
            baseCost = Math.Max(0, baseCost);
            costGrowth = Mathf.Max(1f, costGrowth);
            dnaPerSecondPerLevel = Math.Max(0, dnaPerSecondPerLevel);
        }
    }
}
