using System;

namespace CellClicker.Platform
{
    /// <summary>One <see cref="ISaveable"/>'s data in the save file.</summary>
    [Serializable]
    public class SaveEntry
    {
        public string key;
        public string json;
    }
}
