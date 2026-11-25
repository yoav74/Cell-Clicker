using CellClicker.Core;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Creates the Economy Config asset once and points it at the Tap Power, Organism, Upgrade and Offline
    /// Earnings assets. Only empty links are filled, so later Inspector tweaks are never overwritten.
    /// </summary>
    public static class EconomyConfigDefaults
    {
        private const string Path = ProjectPaths.Data + "/Economy Config.asset";

        public static EconomyConfig GetOrCreate()
        {
            var config = AssetDatabase.LoadAssetAtPath<EconomyConfig>(Path);
            if (config == null)
            {
                ProjectPaths.EnsureFolder(ProjectPaths.Data);
                config = ScriptableObject.CreateInstance<EconomyConfig>();
                AssetDatabase.CreateAsset(config, Path);
            }

            var so = new SerializedObject(config);
            FillIfEmpty(so.FindProperty("tapPower"), TapPowerDefaults.GetOrCreate());
            FillIfEmpty(so.FindProperty("offlineEarnings"), OfflineEarningsDefaults.GetOrCreate());

            SerializedProperty organisms = so.FindProperty("organisms");
            if (organisms.arraySize == 0) FillArray(organisms, OrganismDefaults.GetOrCreate());
            SerializedProperty upgrades = so.FindProperty("upgrades");
            if (upgrades.arraySize == 0) FillArray(upgrades, UpgradeDefaults.GetOrCreate());

            if (so.ApplyModifiedPropertiesWithoutUndo()) EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void FillArray(SerializedProperty array, Object[] values)
        {
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void FillIfEmpty(SerializedProperty property, Object value)
        {
            if (property.objectReferenceValue == null) property.objectReferenceValue = value;
        }
    }
}
