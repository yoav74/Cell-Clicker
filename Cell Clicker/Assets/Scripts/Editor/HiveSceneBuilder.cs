using CellClicker.Core;
using CellClicker.Hive;
using CellClicker.Organisms;
using CellClicker.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Build Hive Scene.
    /// Builds the DNA Hive, HUD, Organisms popup and game systems in the open scene and wires all references.
    /// Running it again replaces the previous build. Tweak everything in the Inspector afterwards.
    ///
    /// Layout (portrait, inside the safe area):
    ///   Header      - fixed height at the top (DNA total with a helix icon, Tap Power and DNA/sec pill)
    ///   Play Area   - everything between header and bottom strip; the hive scales to fit it, so it never covers the HUD
    ///   Bottom strip - Organisms button (bottom-left) and the hint, below the play area so they can't steal hive taps
    /// </summary>
    public static class HiveSceneBuilder
    {
        private const string CanvasName = "Hive Canvas";
        private const string SystemsName = "Game Systems";

        private const float HeaderHeight = 330f;
        private const float BottomStripHeight = 230f;
        private const float PlayAreaGap = 24f;
        private const float HintSideInset = 220f; // keeps the centered hint clear of the Organisms button
        private const float SidePadding = 40f;

        internal static readonly Color Background = new Color32(10, 22, 34, 255);
        private static readonly Color TapColor = new Color32(120, 220, 190, 255);
        private static readonly Color RateColor = new Color32(255, 205, 100, 255);
        private static readonly Color SubtleColor = new Color32(150, 180, 190, 200);
        private static readonly Color PillColor = new Color(0f, 0f, 0f, 0.3f);

        [MenuItem("Cell Clicker/Build Hive Scene")]
        public static void Build() => TmpSetup.RunWhenReady(BuildNow);

        private static void BuildNow()
        {
            MobileProjectSettings.ApplyPortrait();
            RemovePreviousBuild();

            // Game systems
            var systems = new GameObject(SystemsName, typeof(GameBootstrap), typeof(DnaBank), typeof(OrganismRoster),
                typeof(DnaGenerator), typeof(SaveSystem), typeof(ProgressSummary), typeof(OfflineEarningsCollector), typeof(ScreenFlow));
            Undo.RegisterCreatedObjectUndo(systems, "Build Hive Scene");
            var bank = systems.GetComponent<DnaBank>();
            var roster = systems.GetComponent<OrganismRoster>();
            var generator = systems.GetComponent<DnaGenerator>();
            var saveSystem = systems.GetComponent<SaveSystem>();
            var summary = systems.GetComponent<ProgressSummary>();
            var collector = systems.GetComponent<OfflineEarningsCollector>();
            Wire(roster, "bank", bank);
            WireArray(roster, "definitions", OrganismDefaults.GetOrCreate());
            Wire(generator, "bank", bank);
            WireArray(generator, "sources", roster);
            Wire(summary, "bank", bank);
            Wire(summary, "generator", generator);
            WireArray(saveSystem, "saveables", bank, roster, summary);
            Wire(collector, "saveSystem", saveSystem);
            Wire(collector, "generator", generator);
            Wire(collector, "bank", bank);
            Wire(collector, "settings", OfflineEarningsDefaults.GetOrCreate());

            var canvasGo = CreatePortraitCanvas(CanvasName);
            Undo.RegisterCreatedObjectUndo(canvasGo, "Build Hive Scene");
            var canvas = canvasGo.transform;

            // Background fills the whole screen, including behind the notch
            var bg = Create("Background", canvas);
            Stretch(bg);
            AddImage(bg.gameObject, Background);

            // Everything interactive/readable sits inside the safe area
            var safe = Create("Safe Area", canvas);
            Stretch(safe);
            safe.gameObject.AddComponent<CellClicker.UI.SafeArea>();

            // Header. Own canvas: the total changes every tap, so only this small part gets rebuilt.
            var header = PinTop(Create("Header", safe), 0f, HeaderHeight);
            header.gameObject.AddComponent<Canvas>();
            TMP_Text amount = BuildAmountRow(header);
            BuildStatsPill(header, out TMP_Text perTap, out TMP_Text perSecond);

            // Back to Home, top-left: rarely needed, so it sits outside the thumb zone (course guideline).
            Button home = BuildIconButton(safe, "Home Button", "<", Vector2.up, new Vector2(SidePadding, -28f));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(home.onClick, systems.GetComponent<ScreenFlow>().Back);

            // Play area between header and bottom strip
            var playArea = Create("Play Area", safe);
            Stretch(playArea);
            playArea.offsetMin = new Vector2(SidePadding, BottomStripHeight + PlayAreaGap);
            playArea.offsetMax = new Vector2(-SidePadding, -HeaderHeight);

            // The hive: square, as big as the play area allows.
            // Own canvas + raycaster: it animates every frame, so it shouldn't force the rest of the UI to rebuild.
            var hiveRect = Create("DNA Hive", playArea);
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

            // Hint in the bottom strip, centered between the Organisms button and the right edge's mirror
            var hint = Create("Hint", safe);
            hint.anchorMin = Vector2.zero;
            hint.anchorMax = new Vector2(1f, 0f);
            hint.pivot = new Vector2(0.5f, 0f);
            hint.sizeDelta = new Vector2(-2f * HintSideInset, BottomStripHeight);
            hint.anchoredPosition = Vector2.zero;
            AddText(hint.gameObject, "Tap the DNA Hive to make DNA", 38, SubtleColor, TmpSetup.SoftShadowPreset);

            // Popups on top of the HUD, on their own canvas (they move every frame)
            var effects = Create("Effects Layer", canvas);
            Stretch(effects);
            effects.gameObject.AddComponent<Canvas>();
            var spawner = effects.gameObject.AddComponent<FloatingTextSpawner>();

            // Organisms button (bottom-left) and popup (above everything, built last)
            OrganismShopBuilder.Build(canvas, safe, bank, roster);

            var counter = canvasGo.AddComponent<DnaCounterUI>();
            var rate = canvasGo.AddComponent<DnaRateUI>();

            // Wire references
            Wire(hive, "bank", bank);
            Wire(spawner, "hive", hive);
            Wire(spawner, "font", TmpSetup.DefaultFont);
            Wire(spawner, "material", TmpSetup.PopupPreset);
            Wire(counter, "bank", bank);
            Wire(counter, "hive", hive);
            Wire(counter, "amountText", amount);
            Wire(counter, "perTapText", perTap);
            Wire(rate, "generator", generator);
            Wire(rate, "rateText", perSecond);

            EnsureEventSystem();
            SetupCamera();
            HiveVisualPolish.Apply();

            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            Selection.activeGameObject = hiveRect.gameObject;
            Debug.Log("Cell Clicker: hive scene built. Save the scene (Ctrl+S) and press Play.");
        }

        // The DNA total with a helix icon in front of it. The icon replaces a "DNA" title:
        // it reads as "currency" at a glance and keeps the number as the hero.
        private static TMP_Text BuildAmountRow(RectTransform header)
        {
            var row = PinTop(Create("Amount Row", header), 28f, 180f);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 18f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            var icon = Create("DNA Icon", row);
            AddPlaceholder(icon.gameObject, ProceduralSpriteKind.Helix, Color.white);
            var iconSize = icon.gameObject.AddComponent<LayoutElement>();
            iconSize.preferredWidth = 84f;
            iconSize.preferredHeight = 112f;

            var amount = Create("DNA Amount", row);
            return AddText(amount.gameObject, "0", 150, Color.white, TmpSetup.GlowPreset);
        }

        // One pill under the total: Tap Power on the left, DNA/sec on the right.
        private static void BuildStatsPill(RectTransform header, out TMP_Text perTap, out TMP_Text perSecond)
        {
            var pill = Pin(Create("Stats Pill", header), new Vector2(0.5f, 1f), new Vector2(720f, 88f), new Vector2(0f, -224f));
            AddRounded(pill.gameObject, PillColor, 44f);

            var tap = Create("Per Tap", pill);
            Stretch(tap);
            tap.anchorMax = new Vector2(0.5f, 1f);
            tap.offsetMin = new Vector2(24f, 0f);
            tap.offsetMax = new Vector2(-16f, 0f);
            perTap = AddText(tap.gameObject, "+1 per tap", 40, TapColor, TmpSetup.SoftShadowPreset);

            var divider = Pin(Create("Divider", pill), new Vector2(0.5f, 0.5f), new Vector2(4f, 44f), Vector2.zero);
            AddRounded(divider.gameObject, new Color(1f, 1f, 1f, 0.2f), 2f);

            var sec = Create("DNA per Second", pill);
            Stretch(sec);
            sec.anchorMin = new Vector2(0.5f, 0f);
            sec.offsetMin = new Vector2(16f, 0f);
            sec.offsetMax = new Vector2(-24f, 0f);
            perSecond = AddText(sec.gameObject, "0 DNA/sec", 40, RateColor, TmpSetup.SoftShadowPreset);
        }

        private static void RemovePreviousBuild()
        {
            foreach (string name in new[] { CanvasName, SystemsName })
            {
                GameObject old = GameObject.Find(name);
                if (old != null) Undo.DestroyObjectImmediate(old);
            }
        }

        internal static void EnsureEventSystem()
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
        internal static void SetupCamera()
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
    }
}
