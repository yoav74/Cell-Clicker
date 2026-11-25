using CellClicker.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Set Up Economy Config.
    /// Creates the Economy Config asset if needed and assigns it to Bootstrap in the Bootstrap scene,
    /// which hands it to <c>Services.Config</c>. Safe to rerun.
    /// </summary>
    public static class EconomySetup
    {
        private const string BootstrapPath = ProjectPaths.Scenes + "/Bootstrap.unity";

        [MenuItem("Cell Clicker/Set Up Economy Config")]
        public static void Run()
        {
            // Opening Bootstrap closes the open scene, so let the user save it first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EconomyConfig config = EconomyConfigDefaults.GetOrCreate();
            var scene = EditorSceneManager.OpenScene(BootstrapPath, OpenSceneMode.Single);

            var bootstrap = Object.FindAnyObjectByType<Bootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError($"Cell Clicker: no Bootstrap component in {BootstrapPath}.");
                return;
            }

            UiFactory.Wire(bootstrap, "economy", config);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorGUIUtility.PingObject(config);
            Debug.Log("Cell Clicker: Economy Config assigned to Bootstrap. Tune it in Assets/Game/Data/Economy Config.");
        }

        [MenuItem("Cell Clicker/Set Up Economy Config", true)]
        private static bool CanRun() => !EditorApplication.isPlaying;
    }
}
