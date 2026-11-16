using System;
using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// <see cref="ISettingsService"/> stored in PlayerPrefs: settings belong to the device, so they
    /// survive Delete Save and a future cloud-save restore. Only this class touches PlayerPrefs.
    /// </summary>
    public class PlayerPrefsSettings : ISettingsService
    {
        private const string SoundKey = "settings.soundOn";
        private const string HapticsKey = "settings.hapticsOn";

        private bool soundOn;
        private bool hapticsOn;

        public event Action Changed;

        public PlayerPrefsSettings()
        {
            soundOn = Read(SoundKey);
            hapticsOn = Read(HapticsKey);
        }

        public bool SoundOn
        {
            get => soundOn;
            set => Write(ref soundOn, value, SoundKey);
        }

        public bool HapticsOn
        {
            get => hapticsOn;
            set => Write(ref hapticsOn, value, HapticsKey);
        }

        // New players get sound and haptics on.
        private static bool Read(string key) => PlayerPrefs.GetInt(key, 1) == 1;

        private void Write(ref bool field, bool value, string key)
        {
            if (field == value) return;

            field = value;
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save(); // write now: the phone may close us any time
            Changed?.Invoke();
        }
    }
}
