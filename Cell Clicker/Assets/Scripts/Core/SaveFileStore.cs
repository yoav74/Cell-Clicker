using System;
using System.IO;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Reads and writes the save file on disk. Each write goes to a temp file first and the previous save
    /// is kept as a backup, so a crash, a killed app or a full disk mid-write never destroys progress.
    /// </summary>
    public class SaveFileStore
    {
        private readonly string path;
        private readonly string backupPath;
        private readonly string tempPath;

        public SaveFileStore(string path)
        {
            this.path = path;
            backupPath = path + ".bak";
            tempPath = path + ".tmp";
        }

        /// <summary>Loads the save, falling back to the backup if the main file is missing or broken.</summary>
        public bool TryLoad(out SaveFile file) => TryRead(path, out file) || TryRead(backupPath, out file);

        /// <summary>Throws IOException / UnauthorizedAccessException on disk errors; the caller decides what to do.</summary>
        public void Write(SaveFile file)
        {
            File.WriteAllText(tempPath, JsonUtility.ToJson(file));

            if (File.Exists(path))
            {
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(path, backupPath);
            }
            File.Move(tempPath, path);
        }

        public void DeleteAll()
        {
            DeleteIfExists(path);
            DeleteIfExists(backupPath);
            DeleteIfExists(tempPath);
        }

        private static bool TryRead(string filePath, out SaveFile file)
        {
            file = null;
            if (!File.Exists(filePath)) return false;

            try
            {
                file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(filePath));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                // ArgumentException = broken JSON
                Debug.LogWarning($"Cell Clicker: could not read save file {filePath}: {e.Message}");
                return false;
            }

            return file != null && file.entries != null;
        }

        private static void DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
