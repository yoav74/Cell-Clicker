using CellClicker.Core;
using CellClicker.Hive;
using CellClicker.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Build Hive Scene.
    /// Builds the DNA Hive, HUD and popups in the open scene and wires all references.
    /// Running it again replaces the previous build. Tweak everything in the Inspector afterwards.
    ///
    /// Layout (portrait, inside the safe area):
    ///   Header    - fixed height at the top (DNA label, total, per tap)
    ///   Play Area - everything between header and hint; the hive scales to fit it, so it never covers the HUD
    ///   Hint      - fixed at the bottom
    /// </summary>
    public static class HiveSceneBuilder
    {
        private const string CanvasName = "Hive Canvas";
        private const string SystemsName = "Game Systems";

        private const float HeaderHeight = 380f;
        private const float HintHeight = 200f;
        private const float SidePadding = 40f;

        private static readonly Color Background = new Color32(10, 22, 34, 255);
        private static readonly Color LabelColor = new Color32(120, 220, 190, 255);
        private static readonly Color SubtleColor = new Color32(150, 180, 190, 200);

        [MenuItem("Cell Clicker/Build Hive Scene")]
        public static void Build() => TmpSetup.RunWhenReady(BuildNow);

        private static void BuildNow()
        {
            MobileProjectSettings.ApplyPortrait();
            RemovePreviousBuild();

            // Game systems
            var systems = new GameObject(SystemsName, typeof(GameBootstrap), typeof(DnaBank));
            Undo.RegisterCreatedObjectUndo(systems, "Build Hive Scene");
            var bank = systems.GetComponent<DnaBank>();

            // Root canvas, portrait 1080x1920 reference, scaled by width so it fits every phone width
            var canvasGo = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Build Hive Scene");
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            var canvas = canvasGo.transform;

            // Background fills the whole screen, including behind the notch
            var bg = CreateUI("Background", canvas);
            Stretch(bg);
            var bgImage = bg.gameObject.AddComponent<Image>();
            bgImage.color = Background;
            bgImage.raycastTarget = false;

            // Everything interactive/readable sits inside the safe area
            var safe = CreateUI("Safe Area", canvas);
            Stretch(safe);
            safe.gameObject.AddComponent<CellClicker.UI.SafeArea>();

            // Header. Own canvas: the total changes every tap, so only this small part gets rebuilt.
            var header = CreateUI("Header", safe);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, HeaderHeight);
            header.anchoredPosition = Vector2.zero;
            header.gameObject.AddComponent<Canvas>();

            Material shadow = TmpSetup.SoftShadowPreset;
            CreateTopText("DNA Label", header, "DNA", 52, LabelColor, FontStyles.Bold, shadow, -40f);
            TMP_Text amount = CreateTopText("DNA Amount", header, "0", 150, Color.white, FontStyles.Bold, TmpSetup.GlowPreset, -105f);
            TMP_Text perTap = CreateTopText("Per Tap", header, "+1 per tap", 42, SubtleColor, FontStyles.Normal, shadow, -300f);

            // Play area between header and hint
            var playArea = CreateUI("Play Area", safe);
            Stretch(playArea);
            playArea.offsetMin = new Vector2(SidePadding, HintHeight);
            playArea.offsetMax = new Vector2(-SidePadding, -HeaderHeight);

            // The hive: square, as big as the play area allows.
            // Own canvas + raycaster: it animates every frame, so it shouldn't force the rest of the UI to rebuild.
            var hiveRect = CreateUI("DNA Hive", playArea);
            Stretch(hiveRect);
            var fitter = hiveRect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;
            hiveRect.gameObject.AddComponent<Canvas>();
            hiveRect.gameObject.AddComponent<GraphicRaycaster>();
            var hiveImage = hiveRect.gameObject.AddComponent<Image>();
            hiveImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); // edit-mode placeholder; replaced at runtime
            hiveImage.preserveAspect = true;
            var hive = hiveRect.gameObject.AddComponent<DnaHive>();

            // Hint at the bottom
            var hint = CreateUI("Hint", safe);
            hint.anchorMin = new Vector2(0f, 0f);
            hint.anchorMax = new Vector2(1f, 0f);
            hint.pivot = new Vector2(0.5f, 0f);
            hint.sizeDelta = new Vector2(0f, HintHeight);
            hint.anchoredPosition = Vector2.zero;
            AddText(hint.gameObject, "Tap the hive to make DNA", 40, SubtleColor, FontStyles.Normal, shadow);

            // Popups on top of everything, on their own canvas (they move every frame)
            var effects = CreateUI("Effects Layer", canvas);
            Stretch(effects);
            effects.gameObject.AddComponent<Canvas>();
            var spawner = effects.gameObject.AddComponent<FloatingTextSpawner>();

            var counter = canvasGo.AddComponent<DnaCounterUI>();

            // Wire references
            Wire(hive, "bank", bank);
            Wire(spawner, "hive", hive);
            Wire(spawner, "font", TmpSetup.DefaultFont);
            Wire(spawner, "material", TmpSetup.PopupPreset);
            Wire(counter, "bank", bank);
            Wire(counter, "hive", hive);
            Wire(counter, "amountText", amount);
            Wire(counter, "perTapText", perTap);

            EnsureEventSystem();
            SetupCamera();
            HiveVisualPolish.Apply();

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            Selection.activeGameObject = hiveRect.gameObject;
            Debug.Log("Cell Clicker: hive scene built. Save the scene (Ctrl+S) and press Play.");
        }

        private static void RemovePreviousBuild()
        {
            foreach (string name in new[] { CanvasName, SystemsName })
            {
                GameObject old = GameObject.Find(name);
                if (old != null) Undo.DestroyObjectImmediate(old);
            }
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(go, "Build Hive Scene");
                return;
            }

            // The project uses the new Input System only, so the old module would throw errors.
            var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null) Undo.DestroyObjectImmediate(oldModule);
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
        }

        // The game is all overlay UI, so the camera has nothing to draw. Skip 3D rendering and post-processing.
        private static void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Undo.RecordObject(cam, "Build Hive Scene");
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Background;
            cam.cullingMask = 0;

            var urpData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpData != null)
            {
                Undo.RecordObject(urpData, "Build Hive Scene");
                urpData.renderPostProcessing = false;
                urpData.renderShadows = false;
            }
        }

        private static RectTransform CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Creates a text anchored to the top center of its parent.
        private static TMP_Text CreateTopText(string name, Transform parent, string content, float size, Color color, FontStyles style, Material preset, float top)
        {
            var rect = CreateUI(name, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(1000, size * 1.3f);
            rect.anchoredPosition = new Vector2(0f, top);

            return AddText(rect.gameObject, content, size, color, style, preset);
        }

        private static TMP_Text AddText(GameObject go, string content, float size, Color color, FontStyles style, Material preset)
        {
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = TmpSetup.DefaultFont;
            if (preset != null) text.fontSharedMaterial = preset;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
