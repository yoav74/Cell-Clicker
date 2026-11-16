using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// <see cref="ISoundService"/> with a small pool of AudioSources that survives scene changes.
    /// A pool instead of PlayOneShot so every sound can have its own pitch.
    /// </summary>
    public class AudioSourceSound : ISoundService
    {
        private readonly ISettingsService settings;
        private readonly AudioSource[] voices;
        private int nextVoice;

        /// <param name="voiceCount">How many sounds can overlap; the oldest is cut off when they run out.</param>
        public AudioSourceSound(ISettingsService settings, int voiceCount = 8)
        {
            this.settings = settings;

            var host = new GameObject("Sound");
            Object.DontDestroyOnLoad(host);

            voices = new AudioSource[Mathf.Max(1, voiceCount)];
            for (int i = 0; i < voices.Length; i++)
            {
                AudioSource voice = host.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f; // 2D: UI sounds come from everywhere
                voices[i] = voice;
            }

            settings.Changed += StopIfMuted;
        }

        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || !settings.SoundOn) return;

            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;

            voice.clip = clip;
            voice.volume = Mathf.Clamp01(volume);
            voice.pitch = pitch;
            voice.Play();
        }

        // Turning Sound off should silence the game right away, not after the current sounds end.
        private void StopIfMuted()
        {
            if (settings.SoundOn) return;
            foreach (AudioSource voice in voices) voice.Stop();
        }
    }
}
