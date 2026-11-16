namespace CellClicker.UI
{
    /// <summary>Turns all game sounds on or off.</summary>
    public class SoundToggleUI : SettingToggleUI
    {
        protected override bool Value
        {
            get => Settings.SoundOn;
            set => Settings.SoundOn = value;
        }
    }
}
