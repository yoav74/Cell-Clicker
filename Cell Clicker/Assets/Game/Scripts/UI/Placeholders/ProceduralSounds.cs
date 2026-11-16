using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellClicker.UI
{
    public enum ProceduralSoundKind
    {
        Tap,
        Buy,
        Click,
    }

    /// <summary>
    /// Placeholder sounds made in code (short sine blips), generated once and cached, like
    /// <see cref="ProceduralSprites"/>. Every user has a clip field that replaces these with real audio.
    /// </summary>
    public static class ProceduralSounds
    {
        private const int SampleRate = 44100;
        private const float AttackSeconds = 0.003f; // a tiny fade-in so the blip doesn't click

        private static readonly Dictionary<ProceduralSoundKind, AudioClip> Cache = new Dictionary<ProceduralSoundKind, AudioClip>();

        public static AudioClip Get(ProceduralSoundKind kind)
        {
            if (Cache.TryGetValue(kind, out AudioClip clip) && clip != null) return clip;

            clip = kind switch
            {
                ProceduralSoundKind.Tap => Build("Tap", 0.07f, data => Blip(data, 0, data.Length, 880f, 440f, 0.5f)),
                ProceduralSoundKind.Buy => Build("Buy", 0.23f, Chime),
                ProceduralSoundKind.Click => Build("Click", 0.03f, data => Blip(data, 0, data.Length, 1400f, 1100f, 0.35f)),
                _ => null,
            };

            // Runtime-only assets: never saved into the scene.
            if (clip != null) clip.hideFlags = HideFlags.HideAndDontSave;

            Cache[kind] = clip;
            return clip;
        }

        private static AudioClip Build(string name, float seconds, Action<float[]> write)
        {
            var data = new float[Mathf.CeilToInt(seconds * SampleRate)];
            write(data);
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Two rising notes: the "you got something" sound.
        private static void Chime(float[] data)
        {
            int first = Mathf.RoundToInt(data.Length * 0.4f);
            Blip(data, 0, first, 660f, 660f, 0.45f);
            Blip(data, first, data.Length - first, 990f, 990f, 0.45f);
        }

        /// <summary>A sine tone sliding from <paramref name="startHz"/> to <paramref name="endHz"/> with a fast decay.</summary>
        private static void Blip(float[] data, int start, int length, float startHz, float endHz, float gain)
        {
            float phase = 0f;
            float attackSamples = AttackSeconds * SampleRate;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)length;
                float hz = startHz * Mathf.Pow(endHz / startHz, t);
                phase += 2f * Mathf.PI * hz / SampleRate;

                float envelope = Mathf.Min(1f, i / attackSamples) * Mathf.Exp(-5f * t);
                float wave = (Mathf.Sin(phase) + 0.25f * Mathf.Sin(2f * phase)) / 1.25f; // a little overtone for body
                data[start + i] = gain * envelope * wave;
            }
        }
    }
}
