using CellClicker.Core;
using UnityEditor;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>Menu: Cell Clicker > Delete Save / Open Save Folder, for testing a fresh start.</summary>
    public static class SaveMenu
    {
        private const string DeleteItem = "Cell Clicker/Delete Save";

        [MenuItem(DeleteItem)]
        private static void DeleteSave()
        {
            new SaveFileStore(SaveSystem.DefaultPath).DeleteAll();
            Debug.Log($"Cell Clicker: save deleted ({SaveSystem.DefaultPath}).");
        }

        // In Play Mode the SaveSystem would write the file right back.
        [MenuItem(DeleteItem, true)]
        private static bool CanDeleteSave() => !EditorApplication.isPlaying;

        [MenuItem("Cell Clicker/Open Save Folder")]
        private static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
