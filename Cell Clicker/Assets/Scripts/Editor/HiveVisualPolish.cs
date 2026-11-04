using CellClicker.Core;
using CellClicker.Hive;
using CellClicker.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Apply Visual Polish.
    /// Adds the flashy layer (background gradient and motes, hive aura, tap bursts, header panel,
    /// helix icons, pulsing hint) to an existing hive scene. It only adds what's missing and never
    /// deletes anything, so it's safe to run again and keeps your Inspector tweaks.
    /// </summary>
    public static class HiveVisualPolish
    {
        private static readonly Color GlowColor = new Color32(92, 225, 181, 90);
        private static readonly Color RaysColor = new Color32(200, 255, 235, 30);
        private static readonly Color PanelColor = new Color32(255, 255, 255, 14);

        [MenuItem("Cell Clicker/Apply Visual Polish")]
        public static void ApplyFromMenu()
        {
            if (Apply())
            {
                Debug.Log("Cell Clicker: visual polish applied. Save the scene (Ctrl+S) and press Play.");
            }
        }

        public static bool Apply()
        {
            GameObject canvasGo = GameObject.Find("Hive Canvas");
            GameObject systems = GameObject.Find("Game Systems");
            if (canvasGo == null || systems == null)
            {
                EditorUtility.DisplayDialog("Cell Clicker", "No hive scene found. Run Cell Clicker > Build Hive Scene first.", "OK");
                return false;
            }

            Transform canvas = canvasGo.transform;
            Transform safe = Require(canvas, "Safe Area");
            Transform header = Require(safe, "Header");
            Transform playArea = Require(safe, "Play Area");
            Transform hint = Require(safe, "Hint");
            Transform effects = Require(canvas, "Effects Layer");
            Transform background = Require(canvas, "Background");
            Transform amount = Require(header, "DNA Amount");
            if (safe == null || header == null || playArea == null || hint == null || effects == null || background == null || amount == null)
            {
                return false;
            }

            var bank = systems.GetComponent<DnaBank>();
            var hive = playArea.GetComponentInChildren<DnaHive>(true);
            if (hive == null)
            {
                Debug.LogError("Cell Clicker: no DnaHive under 'Play Area'.");
                return false;
            }

            // Background: gradient + drifting motes
            var bgImage = background.GetComponent<UnityEngine.UI.Image>();
            Undo.RecordObject(bgImage, "Apply Visual Polish");
            bgImage.color = Color.white;
            SetKind(GetOrAdd<ProceduralImage>(background.gameObject), ProceduralSpriteKind.Background);

            RectTransform motes = FindOrCreate(canvas, "Background Motes", background.GetSiblingIndex() + 1);
            Stretch(motes);
            GetOrAdd<Canvas>(motes.gameObject);
            GetOrAdd<BackgroundMotes>(motes.gameObject);

            // Draw order inside the safe area: play area first, so the aura never covers the HUD.
            Undo.RegisterFullObjectHierarchyUndo(safe.gameObject, "Apply Visual Polish");
            playArea.SetAsFirstSibling();

            // Hive aura (glow + rays) behind the hive
            RectTransform aura = FindOrCreate(playArea, "Hive Aura", 0);
            Stretch(aura);
            var fitter = GetOrAdd<UnityEngine.UI.AspectRatioFitter>(aura.gameObject);
            fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;
            GetOrAdd<Canvas>(aura.gameObject);

            UnityEngine.UI.Image glow = CreateDecor(aura, "Glow", 0, -0.3f, 1.3f, ProceduralSpriteKind.SoftDot, GlowColor);
            UnityEngine.UI.Image rays = CreateDecor(aura, "Rays", 1, -0.4f, 1.4f, ProceduralSpriteKind.Rays, RaysColor);

            var hiveAura = GetOrAdd<HiveAura>(aura.gameObject);
            Wire(hiveAura, "hive", hive);
            Wire(hiveAura, "glow", glow);
            Wire(hiveAura, "rays", rays);

            // Hive art and squish
            SetKind(GetOrAdd<ProceduralImage>(hive.gameObject), ProceduralSpriteKind.Cell);
            Wire(GetOrAdd<HiveSquish>(hive.gameObject), "hive", hive);

            // Header: glass panel behind the number, helix icons around the label (the number glow is a TMP material)
            RectTransform panel = FindOrCreate(header, "Amount Panel", 0);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.sizeDelta = new Vector2(860f, 230f);
            panel.anchoredPosition = new Vector2(0f, -88f);
            var panelImage = GetOrAdd<UnityEngine.UI.Image>(panel.gameObject);
            panelImage.type = UnityEngine.UI.Image.Type.Sliced;
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;
            SetKind(GetOrAdd<ProceduralImage>(panel.gameObject), ProceduralSpriteKind.Pill);

            CreateHelix(header, "Helix Left", -120f);
            CreateHelix(header, "Helix Right", 120f);

            // Pulsing hint that disappears after the first tap
            var hintPrompt = GetOrAdd<HintPrompt>(hint.gameObject);
            Wire(hintPrompt, "bank", bank);

            // Tap feedback on the effects layer
            Wire(GetOrAdd<FloatingTextSpawner>(effects.gameObject), "hive", hive);
            Wire(GetOrAdd<TapBurstFeedback>(effects.gameObject), "hive", hive);

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            return true;
        }

        private static void CreateHelix(Transform header, string name, float x)
        {
            RectTransform helix = FindOrCreate(header, name, 1);
            helix.anchorMin = helix.anchorMax = new Vector2(0.5f, 1f);
            helix.pivot = new Vector2(0.5f, 1f);
            helix.sizeDelta = new Vector2(54f, 72f);
            helix.anchoredPosition = new Vector2(x, -38f);
            var image = GetOrAdd<UnityEngine.UI.Image>(helix.gameObject);
            image.raycastTarget = false;
            SetKind(GetOrAdd<ProceduralImage>(helix.gameObject), ProceduralSpriteKind.Helix);
        }

        // A non-interactive image whose anchors reach past its parent (e.g. -0.3..1.3 = 160% of the hive).
        private static UnityEngine.UI.Image CreateDecor(Transform parent, string name, int index, float min, float max, ProceduralSpriteKind kind, Color color)
        {
            RectTransform rect = FindOrCreate(parent, name, index);
            rect.anchorMin = new Vector2(min, min);
            rect.anchorMax = new Vector2(max, max);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = GetOrAdd<UnityEngine.UI.Image>(rect.gameObject);
            image.color = color;
            image.raycastTarget = false;
            SetKind(GetOrAdd<ProceduralImage>(rect.gameObject), kind);
            return image;
        }

        private static Transform Require(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null) Debug.LogError($"Cell Clicker: '{parent.name}/{name}' is missing. Rebuild with Cell Clicker > Build Hive Scene.");
            return child;
        }

        private static RectTransform FindOrCreate(Transform parent, string name, int siblingIndex)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                Undo.RecordObject(existing, "Apply Visual Polish");
                return (RectTransform)existing;
            }

            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Apply Visual Polish");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.SetSiblingIndex(siblingIndex);
            return rect;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null) return Undo.AddComponent<T>(go);

            Undo.RecordObject(component, "Apply Visual Polish");
            return component;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetKind(ProceduralImage image, ProceduralSpriteKind kind)
        {
            var so = new SerializedObject(image);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.ApplyModifiedProperties();
            image.enabled = false; // re-enable so the new sprite shows immediately in the editor
            image.enabled = true;
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
