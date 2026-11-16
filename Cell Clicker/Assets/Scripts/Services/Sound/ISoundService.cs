using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>Plays short UI and gameplay sounds. Does nothing while Sound is off in the settings.</summary>
    public interface ISoundService
    {
        /// <param name="pitch">1 = normal. Small random changes keep fast repeated sounds (taps) from sounding robotic.</param>
        void Play(AudioClip clip, float volume = 1f, float pitch = 1f);
    }
}
