using CellClicker.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Build Home Scene.
    /// Renames SampleScene to Level (the hive), creates Assets/Scenes/Home.unity with the Home screen,
    /// and sets the Scene List to Home first (the phone opens the first scene), then Level.
    ///
    /// Layout (portrait, inside the safe area):
    ///   Top    - game title
    ///   Middle - DNA total + DNA/sec, and the "While you were away" card after a break
    ///   Bottom - big Play button in the thumb zone
    /// </summary>
    public static class HomeSceneBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string HomePath = ScenesFolder + "/Home.unity";
        private const string LevelPath = ScenesFolder + "/Level.unity";
        private const string OldLevelPath = ScenesFolder + "/SampleScene.unity";

        private static readonly Vector2 PlayButtonSize = new Vector2(620f, 200f);
        private const float SidePadding = 60f;

        private static readonly Color Gold = new Color32(255, 196, 64, 255);
        private static readonly Color Ink = new Color32(30, 22, 10, 255);
        private static readonly Color Teal = new Color32(110, 235, 195, 255);
        private static readonly Color RateColor = new Color32(255, 205, 100, 255);
        private static readonly Color Subtle = new Color32(150, 180, 190, 220);
        private static readonly Color CardColor = new Color32(255, 255, 255, 18);

        [MenuItem("Cell Clicker/Build Home Scene")]
        public static void Build()
        {
            // Creating a new scene closes the open one, so let the user save it first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            TmpSetup.RunWhenReady(BuildNow);
        }

        private static void BuildNow()
        {
            EnsureLevelScene();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHomeUi();
            HiveSceneBuilder.EnsureEventSystem();
            HiveSceneBuilder.SetupCamera();

            EditorSceneManager.SaveScene(scene, HomePath);
            SetSceneList();
            Debug.Log("Cell Clicker: Home scene built and saved. Scene List = Home, Level. Press Play from Home.");
        }

        private static void BuildHomeUi()
        {
            var flowGo = new GameObject("Screen Flow", typeof(ScreenFlow));
            var flow = flowGo.GetComponent<ScreenFlow>();

            GameObject canvasGo = CreatePortraitCanvas("Home Canvas");
            Transform canvas = canvasGo.transform;

            RectTransform bg = Create("Background", canvas);
            Stretch(bg);
            AddImage(bg.gameObject, HiveSceneBuilder.Background);

            RectTransform safe = Create("Safe Area", canvas);
            Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            RectTransform title = PinTop(Create("Title", safe), 120f, 200f, SidePadding, SidePadding);
            AddText(title.gameObject, "Cell Clicker", 150, Color.white, TmpSetup.GlowPreset);

            RectTransform cell = Pin(Create("Cell", safe), new Vector2(0.5f, 0.62f), new Vector2(380f, 380f), Vector2.zero);
            AddPlaceholder(cell.gameObject, ProceduralSpriteKind.Cell, Teal);

            BuildTotal(safe, out TMP_Text total, out TMP_Text rate);
            GameObject awayCard = BuildAwayCard(safe, out TMP_Text awayAmount, out TMP_Text awayDetail);

            Button play = BuildPlayButton(safe);
            UnityEventTools.AddPersistentListener(play.onClick, flow.Play);

            var home = canvasGo.AddComponent<HomeScreenUI>();
            Wire(home, "offlineSettings", OfflineEarningsDefaults.GetOrCreate());
            Wire(home, "totalText", total);
            Wire(home, "rateText", rate);
            Wire(home, "awayCard", awayCard);
            Wire(home, "awayAmountText", awayAmount);
            Wire(home, "awayDetailText", awayDetail);
        }

        private static void BuildTotal(Transform safe, out TMP_Text total, out TMP_Text rate)
        {
            RectTransform totalRect = Pin(Create("DNA Total", safe), new Vector2(0.5f, 0.45f), new Vector2(900f, 150f), Vector2.zero);
            total = AddText(totalRect.gameObject, "0", 130, Color.white, TmpSetup.GlowPreset);

            RectTransform rateRect = Pin(Create("DNA per Second", safe), new Vector2(0.5f, 0.45f), new Vector2(900f, 60f), new Vector2(0f, -110f));
            rate = AddText(rateRect.gameObject, "0 DNA/sec", 44, RateColor, TmpSetup.SoftShadowPreset);
        }

        private static GameObject BuildAwayCard(Transform safe, out TMP_Text amount, out TMP_Text detail)
        {
            RectTransform card = Pin(Create("Away Card", safe), new Vector2(0.5f, 0f), new Vector2(860f, 250f), new Vector2(0f, 330f));
            AddRounded(card.gameObject, CardColor, 40f);

            RectTransform heading = PinTop(Create("Heading", card), 24f, 56f, 30f, 30f);
            AddText(heading.gameObject, "While you were away", 44, Subtle, TmpSetup.SoftShadowPreset);

            RectTransform amountRect = PinTop(Create("Amount", card), 84f, 96f, 30f, 30f);
            amount = AddText(amountRect.gameObject, "+0 DNA", 84, Gold, TmpSetup.PopupPreset);

            RectTransform detailRect = PinTop(Create("Detail", card), 182f, 48f, 30f, 30f);
            detail = AddText(detailRect.gameObject, "in 0m away", 36, Subtle, TmpSetup.SoftShadowPreset);

            return card.gameObject;
        }

        // The core action, bottom-center in the thumb zone; well above the 140 px minimum.
        private static Button BuildPlayButton(Transform safe)
        {
            RectTransform rect = Pin(Create("Play Button", safe), new Vector2(0.5f, 0f), PlayButtonSize, new Vector2(0f, 90f));
            Image image = AddRounded(rect.gameObject, Gold, PlayButtonSize.y * 0.5f, raycast: true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            RectTransform label = Create("Label", rect);
            Stretch(label);
            AddText(label.gameObject, "PLAY", 96, Ink, null);
            return button;
        }

        // Keeps the existing hive scene (same GUID, so nothing breaks) and gives it the course's name.
        private static void EnsureLevelScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelPath) != null) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OldLevelPath) == null)
            {
                Debug.LogWarning($"Cell Clicker: no {LevelPath} yet. Open a new scene, run Build Hive Scene and save it as Level.");
                return;
            }

            string error = AssetDatabase.MoveAsset(OldLevelPath, LevelPath);
            if (!string.IsNullOrEmpty(error)) Debug.LogError($"Cell Clicker: could not rename SampleScene to Level: {error}");
        }

        private static void SetSceneList()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(HomePath, true),
                new EditorBuildSettingsScene(LevelPath, true),
            };
        }
    }
}
