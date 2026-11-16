using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// <see cref="IHapticsService"/> for Android phones: short, soft ticks through Android's Vibrator
    /// (VibrationEffect, API 26+, our minimum is 29). Unity's own Handheld.Vibrate only has one long buzz,
    /// far too strong for every tap. In the Editor and on other platforms it does nothing.
    /// </summary>
    public class AndroidHaptics : IHapticsService
    {
        private readonly ISettingsService settings;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Duration (ms) and amplitude (1-255) per HapticStrength. Short and soft: the player taps many times a second.
        private static readonly long[] Durations = { 12, 25, 45 };
        private static readonly int[] Amplitudes = { 60, 140, 255 };
        private const int DefaultAmplitude = -1; // VibrationEffect.DEFAULT_AMPLITUDE, for phones without amplitude control

        private AndroidJavaObject vibrator;
        private AndroidJavaObject[] effects; // made once, so a tap doesn't create Java objects
#endif

        public AndroidHaptics(ISettingsService settings)
        {
            this.settings = settings;
#if UNITY_ANDROID && !UNITY_EDITOR
            Connect();
#endif
        }

        public void Pulse(HapticStrength strength)
        {
            if (!settings.HapticsOn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (vibrator == null)
            {
                // No fine control: only big moments get Unity's long buzz. Using Handheld.Vibrate also makes
                // Unity add the VIBRATE permission to the build, which the Vibrator calls need too.
                if (strength == HapticStrength.Heavy) Handheld.Vibrate();
                return;
            }

            try
            {
                vibrator.Call("vibrate", effects[(int)strength]);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogWarning($"Cell Clicker: vibration failed, haptics disabled for this session: {e.Message}");
                vibrator = null;
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void Connect()
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject found = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (found == null || !found.Call<bool>("hasVibrator")) return;

                bool amplitudeControl = found.Call<bool>("hasAmplitudeControl");
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                effects = new AndroidJavaObject[Durations.Length];
                for (int i = 0; i < effects.Length; i++)
                {
                    int amplitude = amplitudeControl ? Amplitudes[i] : DefaultAmplitude;
                    effects[i] = effectClass.CallStatic<AndroidJavaObject>("createOneShot", Durations[i], amplitude);
                }
                vibrator = found;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogWarning($"Cell Clicker: no access to the vibrator: {e.Message}");
            }
        }
#endif
    }
}
