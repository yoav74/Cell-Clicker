using CellClicker.Core;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Creates the starting Upgrade assets (Cell Evolution Stage) if they don't exist yet.
    /// Existing assets are never overwritten, so balance changes made in the Inspector are kept.
    /// </summary>
    public static class UpgradeDefaults
    {
        private const string Folder = ProjectPaths.Upgrades;

        public static UpgradeDefinition[] GetOrCreate()
        {
            ProjectPaths.EnsureFolder(Folder);
            var upgrades = new UpgradeDefinition[]
            {
                // The economy sheet's curve: the big sink. x3 per level keeps pace with income, which grows ~x4 per new Organism.
                GetOrCreate<AllDnaUpgradeDefinition>("01 Richer DNA", "Richer DNA", "richerDna",
                    baseCost: 3000, growth: 3f, maxLevel: 10, amount: 0.25f, new Color32(205, 150, 255, 255)),
                GetOrCreate<TapPowerUpgradeDefinition>("02 Stronger Taps", "Stronger Taps", "strongerTaps",
                    baseCost: 1000, growth: 3f, maxLevel: 10, amount: 0.5f, new Color32(255, 196, 64, 255)),
                // Multiplied per level: 10 levels = 40% off every Organism Level.
                GetOrCreate<OrganismCostUpgradeDefinition>("03 Cheaper Organisms", "Cheaper Organisms", "cheaperOrganisms",
                    baseCost: 5000, growth: 4f, maxLevel: 10, amount: 0.05f, new Color32(120, 235, 190, 255)),
            };
            AssetDatabase.SaveAssets();
            return upgrades;
        }

        private static T GetOrCreate<T>(string fileName, string displayName, string saveId,
            double baseCost, float growth, int maxLevel, float amount, Color tint) where T : UpgradeDefinition
        {
            string path = $"{Folder}/{fileName}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<T>(path);
            if (definition != null) return definition;

            definition = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(definition, path);

            var so = new SerializedObject(definition);
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("saveId").stringValue = saveId;
            so.FindProperty("tint").colorValue = tint;
            so.FindProperty("baseCost").doubleValue = baseCost;
            so.FindProperty("costGrowth").floatValue = growth;
            so.FindProperty("maxLevel").intValue = maxLevel;
            so.FindProperty("amountPerLevel").floatValue = amount;
            so.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }
    }
}
