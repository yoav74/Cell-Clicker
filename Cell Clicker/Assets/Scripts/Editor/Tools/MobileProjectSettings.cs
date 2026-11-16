using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Apply Portrait Settings.
    /// Locks the game to portrait (one-thumb play). Uses the PlayerSettings API instead of
    /// hand-editing ProjectSettings.asset, so it is safe while the editor is open.
    /// </summary>
    public static class MobileProjectSettings
    {
        [MenuItem("Cell Clicker/Apply Portrait Settings")]
        public static void ApplyPortrait()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            AssetDatabase.SaveAssets();
            Debug.Log("Cell Clicker: orientation locked to Portrait. Set the Game view to a portrait size (e.g. 1080x1920) or use the Simulator view.");
        }
    }
}
