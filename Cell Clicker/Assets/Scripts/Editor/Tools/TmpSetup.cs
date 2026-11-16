using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// TextMeshPro helpers for the editor tools: imports TMP Essential Resources when missing,
    /// creates the game font asset (Lilita One) and the game's text material presets.
    /// </summary>
    public static class TmpSetup
    {
        private const string FallbackFontResourcePath = "Fonts & Materials/LiberationSans SDF";
        private const string MobileShaderName = "TextMeshPro/Mobile/Distance Field";
        private const string PresetFolder = "Assets/Art/Fonts";

        private const string GameFontSource = "Assets/Art/Fonts/LilitaOne-Regular.ttf";
        private const string GameFontName = "LilitaOne SDF";
        private const string GameFontPath = "Assets/Art/Fonts/" + GameFontName + ".asset";

        // Added to the atlas up front so the UI shows right away; anything else is added on demand.
        private const string PrefilledCharacters =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

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

        /// <summary>The game font (Lilita One), created on first use. Falls back to LiberationSans if the .ttf is missing.</summary>
        public static TMP_FontAsset DefaultFont => GetOrCreateGameFont() ?? FallbackFont;

        private static TMP_FontAsset FallbackFont => Resources.Load<TMP_FontAsset>(FallbackFontResourcePath);

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

        // Dynamic SDF asset: sharp at any size, adds missing glyphs (e.g. other languages) on demand.
        private static TMP_FontAsset GetOrCreateGameFont()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GameFontPath);
            if (existing != null) return existing;

            var source = AssetDatabase.LoadAssetAtPath<Font>(GameFontSource);
            if (source == null)
            {
                Debug.LogWarning($"Cell Clicker: {GameFontSource} not found, using LiberationSans. Is Git LFS installed?");
                return null;
            }

            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, 72, 9, GlyphRenderMode.SDFAA, 1024, 1024);
            if (font == null)
            {
                Debug.LogError($"Cell Clicker: could not create a font asset from {GameFontSource}.");
                return null;
            }

            // Same steps as TMP's own "Create > TextMeshPro > Font Asset": the atlas and material live inside the asset.
            font.name = GameFontName;
            AssetDatabase.CreateAsset(font, GameFontPath);
            font.atlasTextures[0].name = GameFontName + " Atlas";
            AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
            font.material.name = GameFontName + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);

            font.TryAddCharacters(PrefilledCharacters, out string missing);
            if (!string.IsNullOrEmpty(missing)) Debug.LogWarning($"Cell Clicker: {GameFontName} has no glyphs for: {missing}");

            TMP_FontAsset fallback = FallbackFont;
            if (fallback != null) font.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            return font;
        }

        // Created once per font; after that the asset is reused so Inspector tweaks are kept.
        // The name starts with the font name so TMP's Inspector lists it in the "Material Preset" dropdown.
        private static Material GetOrCreatePreset(string name, Action<Material> configure)
        {
            TMP_FontAsset font = DefaultFont;
            if (font == null)
            {
                Debug.LogError("Cell Clicker: no TMP font found. Are TMP Essential Resources imported?");
                return null;
            }

            string presetName = $"{font.name} - {name}";
            string path = $"{PresetFolder}/{presetName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader mobile = Shader.Find(MobileShaderName);
            if (mobile == null)
            {
                Debug.LogError("Cell Clicker: TMP mobile shader missing. Are TMP Essential Resources imported?");
                return null;
            }

            // Copy the font's material so the atlas texture and SDF settings match, then switch to the cheaper mobile shader.
            var material = new Material(font.material) { name = presetName, shader = mobile };
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
