using System;

namespace CellClicker.Platform
{
    /// <summary>
    /// Where the game keeps progress between sessions. Systems store their own JSON under their own key;
    /// the service decides where it lives (a local file today, maybe a cloud save later).
    /// </summary>
    public interface ISaveService
    {
        /// <summary>When the save was last written to storage, or null for a new player. For Offline Earnings.</summary>
        DateTime? LastSavedUtc { get; }

        /// <summary>The JSON stored under <paramref name="key"/>, or null if there is none.</summary>
        string Load(string key);

        /// <summary>Keeps the JSON in memory; call <see cref="Flush"/> to write it to storage.</summary>
        void Store(string key, string json);

        /// <summary>Writes everything stored so far. False if writing failed; the game keeps playing and tries again later.</summary>
        bool Flush();

        /// <summary>Erases all progress, in memory and in storage.</summary>
        void DeleteAll();
    }
}
