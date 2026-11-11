namespace CellClicker.Core
{
    /// <summary>
    /// Anything whose state is kept between sessions. Each one stores its own JSON under its own key,
    /// so adding a new saved system never touches the save code or the other systems' data.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Stable name in the save file. Never change it after release, or old saves lose this data.</summary>
        string SaveKey { get; }

        string CaptureState();

        /// <summary>Called once at startup, only when the save file has data for <see cref="SaveKey"/>.</summary>
        void RestoreState(string json);
    }
}
