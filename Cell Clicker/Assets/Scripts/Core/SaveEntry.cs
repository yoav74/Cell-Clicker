using System;

namespace CellClicker.Core
{
    /// <summary>One <see cref="ISaveable"/>'s data in the save file.</summary>
    [Serializable]
    public class SaveEntry
    {
        public string key;
        public string json;
    }
}
