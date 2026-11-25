using System.IO;
using CellClicker.Hive;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Create App Icon.
    /// Draws the DNA Hive (the same code-drawn art as in the game) on the game's dark background, saves it as
    /// Assets/Game/Art/Icons/App Icon.png and sets it as the Default Icon in Player Settings. Safe to rerun.
    /// </summary>
    public static class AppIconTool
    {
        private const int Size = 1024;          // stores ask for 1024x1024; Unity scales it down per device
        private const float HiveScale = 0.92f;  // share of the icon the hive covers; launchers crop the corners
        private static readonly Color32 Background = new Color32(10, 22, 34, 255); // same as the Level background
        private static readonly Color32 BackgroundCenter = new Color32(22, 52, 66, 255);

        private const string Folder = ProjectPaths.Game + "/Art/Icons";
        private const string IconPath = Folder + "/App Icon.png";

        [MenuItem("Cell Clicker/Create App Icon")]
        public static void Create()
        {
            ProjectPaths.EnsureFolder(Folder);
            File.WriteAllBytes(IconPath, DrawIcon().EncodeToPNG());
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = Size;
            importer.textureCompression = TextureImporterCompression.Uncompressed; // icons are baked at build time
            importer.SaveAndReimport();

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any); // "Default Icon"
            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(icon);
            Debug.Log($"Cell Clicker: app icon saved to {IconPath} and set as the Default Icon.");
        }

        // Opaque background with a soft lighter center, then the hive on top.
        private static Texture2D DrawIcon()
        {
            int hiveSize = Mathf.RoundToInt(Size * HiveScale);
            Color32[] hive = CellSpriteFactory.Draw(hiveSize);
            int offset = (Size - hiveSize) / 2;

            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float dx = (x + 0.5f) / Size * 2f - 1f;
                    float dy = (y + 0.5f) / Size * 2f - 1f;
                    Color color = Color.Lerp(BackgroundCenter, Background, Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy)));

                    int hx = x - offset;
                    int hy = y - offset;
                    if (hx >= 0 && hy >= 0 && hx < hiveSize && hy < hiveSize)
                    {
                        Color top = hive[hy * hiveSize + hx];
                        color = Color.Lerp(color, new Color(top.r, top.g, top.b, 1f), top.a);
                    }
                    pixels[y * Size + x] = color;
                }
            }

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}
