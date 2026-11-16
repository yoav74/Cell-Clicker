using System;
using System.IO;
using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// <see cref="ISaveService"/> backed by <c>save.json</c> in <see cref="Application.persistentDataPath"/>.
    /// Reads the file once when created and keeps it in memory, so every scene sees the same, latest data.
    /// </summary>
    public class JsonFileSave : ISaveService
    {
        public const string FileName = "save.json";

        private readonly SaveFileStore store;
        private SaveFile file;

        public static string DefaultPath => Path.Combine(Application.persistentDataPath, FileName);

        public DateTime? LastSavedUtc { get; private set; }

        public JsonFileSave() : this(DefaultPath) { }

        public JsonFileSave(string path)
        {
            store = new SaveFileStore(path);
            file = store.TryLoad(out SaveFile saved) ? saved : new SaveFile();
            if (TryGetUtc(file.savedAtUtcTicks, out DateTime savedAt)) LastSavedUtc = savedAt;
        }

        public string Load(string key) => file.Find(key);

        public void Store(string key, string json) => file.Set(key, json);

        public bool Flush()
        {
            DateTime now = DateTime.UtcNow;
            file.version = SaveFile.CurrentVersion;
            file.savedAtUtcTicks = now.Ticks;

            try
            {
                store.Write(file);
                LastSavedUtc = now;
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Cell Clicker: could not save: {e.Message}");
                return false;
            }
        }

        public void DeleteAll()
        {
            store.DeleteAll();
            file = new SaveFile();
            LastSavedUtc = null;
        }

        /// <summary>False if the saved ticks are missing or out of range (broken file).</summary>
        private static bool TryGetUtc(long ticks, out DateTime utc)
        {
            bool valid = ticks > DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks;
            utc = valid ? new DateTime(ticks, DateTimeKind.Utc) : default;
            return valid;
        }
    }
}
