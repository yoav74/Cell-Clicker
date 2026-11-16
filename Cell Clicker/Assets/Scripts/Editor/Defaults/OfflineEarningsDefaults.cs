using CellClicker.Core;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>Creates the Offline Earnings settings asset once; later Inspector tweaks are never overwritten.</summary>
    public static class OfflineEarningsDefaults
    {
        private const string Path = "Assets/Settings/Offline Earnings.asset";

        public static OfflineEarningsSettings GetOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<OfflineEarningsSettings>(Path);
            if (settings != null) return settings;

            if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
            settings = ScriptableObject.CreateInstance<OfflineEarningsSettings>();
            AssetDatabase.CreateAsset(settings, Path);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
