namespace CellClicker.Platform
{
    /// <summary>
    /// The game's services, created once by the Bootstrap scene before any screen opens.
    /// Screens and systems use these instead of PlayerPrefs, files or platform APIs directly.
    /// </summary>
    public static class Services
    {
        public static ISettingsService Settings { get; set; }
        public static ISaveService Save { get; set; }
        public static ISoundService Sound { get; set; }
        public static IHapticsService Haptics { get; set; }
    }
}
