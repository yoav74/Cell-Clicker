using CellClicker.Hive;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Add Sound and Haptics.
    /// Adds the tap sound and tap vibration to the hive in the open Level scene. It only adds what's
    /// missing, so it's safe to run again and keeps your Inspector tweaks. Build Hive Scene adds them too.
    /// </summary>
    public static class HiveFeedbackSetup
    {
        [MenuItem("Cell Clicker/Add Sound and Haptics")]
        private static void AddFromMenu()
        {
            var hive = Object.FindAnyObjectByType<DnaHive>(FindObjectsInactive.Include);
            if (hive == null)
            {
                EditorUtility.DisplayDialog("Cell Clicker", "No DNA Hive in the open scene. Open the Level scene first.", "OK");
                return;
            }

            AddTo(hive);
            EditorSceneManager.MarkSceneDirty(hive.gameObject.scene);
            Debug.Log("Cell Clicker: tap sound and haptics added to the hive. Save the scene (Ctrl+S).");
        }

        public static void AddTo(DnaHive hive)
        {
            AddFeedback<HiveTapSound>(hive);
            AddFeedback<HiveTapHaptics>(hive);
        }

        private static void AddFeedback<T>(DnaHive hive) where T : HiveTapFeedback
        {
            if (!hive.TryGetComponent(out T feedback)) feedback = Undo.AddComponent<T>(hive.gameObject);
            Wire(feedback, "hive", hive);
        }
    }
}
