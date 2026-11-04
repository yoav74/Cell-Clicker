using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// TextMeshPro helpers for the editor tools: imports TMP Essential Resources when missing,
    /// gives the default font, and creates the game's text material presets.
    /// </summary>
    public static class TmpSetup
    {
        private const string FontResourcePath = "Fonts & Materials/LiberationSans SDF";
        private const string MobileShaderName = "TextMeshPro/Mobile/Distance Field";
        private const string PresetFolder = "Assets/Art/Fonts";

        // Preset names must start with the font name so TMP's Inspector lists them in its "Material Preset" dropdown.
        private const string PresetPrefix = "LiberationSans SDF - ";

        /// <summary>
        /// Runs <paramref name="action"/> now if TMP Essential Resources exist, otherwise imports them
        /// silently first and runs it once the import finishes.
        /// </summary>
        public static void RunWhenReady(Action action)
        {
            // Resources.Load, not TMP_Settings.instance: the latter can pop up TMP's importer window.
            if (Resources.Load<TMP_Settings>("TMP Settings") != null)
            {
                action();
                return;
            }

            void OnCompleted(string packageName)
            {
                Unsubscribe();
                AssetDatabase.Refresh();
                Debug.Log("Cell Clicker: TMP Essential Resources imported.");
                action();
            }

            void OnFailed(string packageName, string error)
            {
                Unsubscribe();
                Debug.LogError($"Cell Clicker: importing TMP Essential Resources failed: {error}");
            }

            void OnCancelled(string packageName) => Unsubscribe();

            void Unsubscribe()
            {
                AssetDatabase.importPackageCompleted -= OnCompleted;
                AssetDatabase.importPackageFailed -= OnFailed;
                AssetDatabase.importPackageCancelled -= OnCancelled;
            }

            AssetDatabase.importPackageCompleted += OnCompleted;
            AssetDatabase.importPackageFailed += OnFailed;
            AssetDatabase.importPackageCancelled += OnCancelled;

            Debug.Log("Cell Clicker: importing TMP Essential Resources...");
            TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, interactive: false);
        }

        public static TMP_FontAsset DefaultFont => Resources.Load<TMP_FontAsset>(FontResourcePath);

        /// <summary>Big soft teal glow + thin dark outline, for the DNA total.</summary>
        public static Material GlowPreset => GetOrCreatePreset("DNA Glow", m =>
        {
            SetOutline(m, new Color32(8, 50, 45, 255), 0.12f);
            SetUnderlay(m, new Color32(92, 225, 181, 150), Vector2.zero, dilate: 0.6f, softness: 0.7f);
        });

        /// <summary>Thick dark outline + drop shadow, so popups read over anything.</summary>
        public static Material PopupPreset => GetOrCreatePreset("Popup", m =>
        {
            SetOutline(m, new Color32(10, 20, 30, 230), 0.25f);
            SetUnderlay(m, new Color32(0, 0, 0, 120), new Vector2(0.5f, -0.5f), dilate: 0.2f, softness: 0.3f);
        });

        /// <summary>Subtle drop shadow for labels and hints.</summary>
        public static Material SoftShadowPreset => GetOrCreatePreset("Soft Shadow", m =>
        {
            SetUnderlay(m, new Color32(0, 0, 0, 110), new Vector2(0.4f, -0.4f), dilate: 0f, softness: 0.4f);
        });

        // Created once; after that the asset is reused so Inspector tweaks are kept.
        private static Material GetOrCreatePreset(string name, Action<Material> configure)
        {
            string path = $"{PresetFolder}/{PresetPrefix}{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            TMP_FontAsset font = DefaultFont;
            Shader mobile = Shader.Find(MobileShaderName);
            if (font == null || mobile == null)
            {
                Debug.LogError("Cell Clicker: TMP font or mobile shader missing. Are TMP Essential Resources imported?");
                return null;
            }

            // Copy the font's material so the atlas texture and SDF settings match, then switch to the cheaper mobile shader.
            var material = new Material(font.material) { name = PresetPrefix + name, shader = mobile };
            configure(material);
            ShaderUtilities.UpdateShaderRatios(material);

            EnsureFolder(PresetFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SetOutline(Material m, Color color, float width)
        {
            m.EnableKeyword("OUTLINE_ON");
            m.SetColor("_OutlineColor", color);
            m.SetFloat("_OutlineWidth", width);
        }

        private static void SetUnderlay(Material m, Color color, Vector2 offset, float dilate, float softness)
        {
            m.EnableKeyword("UNDERLAY_ON");
            m.SetColor("_UnderlayColor", color);
            m.SetFloat("_UnderlayOffsetX", offset.x);
            m.SetFloat("_UnderlayOffsetY", offset.y);
            m.SetFloat("_UnderlayDilate", dilate);
            m.SetFloat("_UnderlaySoftness", softness);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
