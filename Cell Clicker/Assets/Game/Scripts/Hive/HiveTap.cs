using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>Everything a feedback effect needs to know about one tap on the hive.</summary>
    public readonly struct HiveTap
    {
        public readonly Vector2 ScreenPosition;
        public readonly Camera EventCamera;
        public readonly double Amount;

        public HiveTap(Vector2 screenPosition, Camera eventCamera, double amount)
        {
            ScreenPosition = screenPosition;
            EventCamera = eventCamera;
            Amount = amount;
        }
    }
}
