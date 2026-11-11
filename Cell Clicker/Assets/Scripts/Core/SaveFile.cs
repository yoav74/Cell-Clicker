using System;
using System.Collections.Generic;

namespace CellClicker.Core
{
    /// <summary>
    /// The whole save file. A list of key/JSON pairs instead of fixed fields, because JsonUtility can't
    /// serialize dictionaries and new systems must be able to add data without changing this class.
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public long savedAtUtcTicks;
        public List<SaveEntry> entries = new List<SaveEntry>();

        /// <summary>The JSON stored under <paramref name="key"/>, or null if there is none.</summary>
        public string Find(string key)
        {
            foreach (SaveEntry entry in entries)
            {
                if (entry.key == key) return entry.json;
            }
            return null;
        }

        public void Set(string key, string json)
        {
            foreach (SaveEntry entry in entries)
            {
                if (entry.key != key) continue;

                entry.json = json;
                return;
            }
            entries.Add(new SaveEntry { key = key, json = json });
        }
    }
}
