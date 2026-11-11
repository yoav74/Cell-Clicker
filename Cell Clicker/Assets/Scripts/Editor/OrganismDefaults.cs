using CellClicker.Organisms;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Creates the starting Organism assets for the Cell Evolution Stage if they don't exist yet.
    /// Existing assets are never overwritten, so balance changes made in the Inspector are kept.
    /// </summary>
    public static class OrganismDefaults
    {
        private const string Folder = "Assets/Settings/Organisms";

        private readonly struct Entry
        {
            public readonly string Name;
            public readonly double Cost;
            public readonly double FirstDnaPerSecond;
            public readonly double DnaPerSecondPerLevel;
            public readonly Color Tint;

            public Entry(string name, double cost, double firstDnaPerSecond, double dnaPerSecondPerLevel, Color tint)
            {
                Name = name;
                Cost = cost;
                FirstDnaPerSecond = firstDnaPerSecond;
                DnaPerSecondPerLevel = dnaPerSecondPerLevel;
                Tint = tint;
            }
        }

        // Each Organism costs ~10x the last and earns 4x; each level after the first adds half the first level.
        private static readonly Entry[] Entries =
        {
            new Entry("Bacterium", 15, 1, 0.5, new Color32(120, 235, 190, 255)),
            new Entry("Amoeba", 100, 4, 2, new Color32(140, 195, 255, 255)),
            new Entry("Paramecium", 1100, 16, 8, new Color32(255, 205, 110, 255)),
            new Entry("Euglena", 12000, 64, 32, new Color32(150, 240, 120, 255)),
            new Entry("Volvox", 130000, 256, 128, new Color32(205, 150, 255, 255)),
        };

        public static OrganismDefinition[] GetOrCreate()
        {
            EnsureFolder();
            var definitions = new OrganismDefinition[Entries.Length];

            for (int i = 0; i < Entries.Length; i++)
            {
                Entry entry = Entries[i];
                string path = $"{Folder}/{i + 1:00} {entry.Name}.asset";

                var definition = AssetDatabase.LoadAssetAtPath<OrganismDefinition>(path);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<OrganismDefinition>();
                    AssetDatabase.CreateAsset(definition, path);

                    var so = new SerializedObject(definition);
                    so.FindProperty("displayName").stringValue = entry.Name;
                    so.FindProperty("saveId").stringValue = entry.Name.ToLowerInvariant();
                    so.FindProperty("tint").colorValue = entry.Tint;
                    so.FindProperty("baseCost").doubleValue = entry.Cost;
                    so.FindProperty("firstLevelDnaPerSecond").doubleValue = entry.FirstDnaPerSecond;
                    so.FindProperty("dnaPerSecondPerLevel").doubleValue = entry.DnaPerSecondPerLevel;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                definitions[i] = definition;
            }

            AssetDatabase.SaveAssets();
            return definitions;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Settings", "Organisms");
        }
    }
}
