namespace CellClicker.Platform
{
    /// <summary>Short vibrations as feedback. Does nothing while Haptics is off or the device can't vibrate.</summary>
    public interface IHapticsService
    {
        void Pulse(HapticStrength strength);
    }
}
