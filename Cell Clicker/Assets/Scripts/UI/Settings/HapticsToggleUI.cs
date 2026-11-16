namespace CellClicker.UI
{
    /// <summary>Turns vibration feedback on or off.</summary>
    public class HapticsToggleUI : SettingToggleUI
    {
        protected override bool Value
        {
            get => Settings.HapticsOn;
            set => Settings.HapticsOn = value;
        }
    }
}
