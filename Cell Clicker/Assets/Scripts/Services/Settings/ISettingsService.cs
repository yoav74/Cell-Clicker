using System;

namespace CellClicker.Platform
{
    /// <summary>The player's on/off preferences. Changes are kept right away and survive closing the app.</summary>
    public interface ISettingsService
    {
        bool SoundOn { get; set; }
        bool HapticsOn { get; set; }

        /// <summary>Fired after any setting changes, so toggles and services can update.</summary>
        event Action Changed;
    }
}
