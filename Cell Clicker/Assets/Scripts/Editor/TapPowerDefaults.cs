using CellClicker.Hive;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>Creates the Tap Power asset once; later Inspector tweaks are never overwritten.</summary>
    public static class TapPowerDefaults
    {
        private const string Path = "Assets/Settings/Tap Power.asset";

        public static TapPowerDefinition GetOrCreate()
        {
            var definition = AssetDatabase.LoadAssetAtPath<TapPowerDefinition>(Path);
            if (definition != null) return definition;

            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            definition = ScriptableObject.CreateInstance<TapPowerDefinition>();
            AssetDatabase.CreateAsset(definition, Path);
            AssetDatabase.SaveAssets();
            return definition;
        }
    }
}
