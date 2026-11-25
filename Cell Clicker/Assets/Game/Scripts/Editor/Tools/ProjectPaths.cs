using UnityEditor;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Where the game's own assets live. Everything we make is under Assets/Game; the rest of Assets
    /// belongs to Unity and packages. Editor tools take their folders from here, so a move is one change.
    /// </summary>
    public static class ProjectPaths
    {
        public const string Game = "Assets/Game";
        public const string Data = Game + "/Data";
        public const string Organisms = Data + "/Organisms";
        public const string Upgrades = Data + "/Upgrades";
        public const string Scenes = Game + "/Scenes";
        public const string Fonts = Game + "/Art/Fonts";

        /// <summary>Creates the folder and any missing parent folders.</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
