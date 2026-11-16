using System.Collections.Generic;
using CellClicker.Hive;
using UnityEngine;

namespace CellClicker.UI
{
    public enum ProceduralSpriteKind
    {
        Cell,
        SoftDot,
        Ring,
        Rays,
        Background,
        Helix,
        Pill,
        Microbe,
    }

    /// <summary>
    /// Placeholder art drawn in code, generated once and cached.
    /// Swap for real sprites later; nothing else needs to change.
    /// </summary>
    public static class ProceduralSprites
    {
        private static readonly Dictionary<ProceduralSpriteKind, Sprite> Cache = new Dictionary<ProceduralSpriteKind, Sprite>();

        public static Sprite Get(ProceduralSpriteKind kind)
        {
            if (Cache.TryGetValue(kind, out Sprite sprite) && sprite != null) return sprite;

            sprite = kind switch
            {
                ProceduralSpriteKind.Cell => CellSpriteFactory.Create(),
                ProceduralSpriteKind.SoftDot => Draw(64, 64, SoftDot),
                ProceduralSpriteKind.Ring => Draw(128, 128, Ring),
                ProceduralSpriteKind.Rays => Draw(256, 256, Rays),
                ProceduralSpriteKind.Background => Draw(128, 256, Background),
                ProceduralSpriteKind.Helix => Draw(96, 128, Helix),
                ProceduralSpriteKind.Pill => Draw(64, 64, Pill, new Vector4(31, 31, 31, 31)),
                ProceduralSpriteKind.Microbe => Draw(128, 128, Microbe),
                _ => null,
            };

            // Runtime-only assets: never saved into the scene.
            if (sprite != null)
            {
                sprite.hideFlags = HideFlags.HideAndDontSave;
                sprite.texture.hideFlags = HideFlags.HideAndDontSave;
            }

            Cache[kind] = sprite;
            return sprite;
        }

        // ---------- Shapes. p is in [-1, 1] on both axes; aa is one pixel in that space. ----------

        private static Color SoftDot(Vector2 p, float aa)
        {
            float a = 1f - Mathf.Clamp01(p.magnitude);
            return new Color(1f, 1f, 1f, a * a);
        }

        private static Color Ring(Vector2 p, float aa)
        {
            float d = Mathf.Abs(p.magnitude - 0.85f);
            return new Color(1f, 1f, 1f, 1f - SmoothStep(0.07f - aa, 0.07f + aa, d));
        }

        private static Color Rays(Vector2 p, float aa)
        {
            const int rayCount = 12;
            float d = p.magnitude;
            float angle = Mathf.Atan2(p.y, p.x);
            float ray = SmoothStep(0.55f, 0.75f, Mathf.Cos(angle * rayCount));
            float fade = SmoothStep(0.15f, 0.35f, d) * (1f - SmoothStep(0.6f, 1f, d));
            return new Color(1f, 1f, 1f, ray * fade);
        }

        private static Color Background(Vector2 p, float aa)
        {
            Color center = new Color32(24, 58, 70, 255);
            Color edge = new Color32(6, 12, 22, 255);
            float d = new Vector2(p.x * 1.1f, (p.y - 0.05f) * 0.6f).magnitude;
            return Color.Lerp(center, edge, SmoothStep(0f, 1f, d));
        }

        private static Color Helix(Vector2 p, float aa)
        {
            Color strandA = new Color32(110, 235, 195, 255);
            Color strandB = new Color32(170, 125, 255, 255);
            Color rung = new Color32(242, 193, 78, 255);

            const float amplitude = 0.55f;
            const float frequency = 5.5f;
            const float width = 0.11f;

            float wave = Mathf.Sin(p.y * frequency);
            float xA = amplitude * wave;
            float xB = -xA;
            float fadeEnds = 1f - SmoothStep(0.85f, 1f, Mathf.Abs(p.y));

            Color c = Color.clear;

            // Rungs: horizontal bars between the strands.
            float rungPhase = Mathf.Repeat(p.y * frequency / Mathf.PI * 2f, 1f);
            float rungLine = 1f - SmoothStep(0.12f, 0.2f, Mathf.Abs(rungPhase - 0.5f));
            float between = Mathf.Abs(p.x) < Mathf.Abs(xA) ? 1f : 0f;
            c = Over(c, rung, rungLine * between * 0.8f);

            // The strand in front is drawn last (depends on the wave's direction).
            bool aInFront = Mathf.Cos(p.y * frequency) > 0f;
            float a = 1f - SmoothStep(width - aa, width + aa, Mathf.Abs(p.x - xA));
            float b = 1f - SmoothStep(width - aa, width + aa, Mathf.Abs(p.x - xB));
            if (aInFront)
            {
                c = Over(c, strandB * 0.7f, b);
                c = Over(c, strandA, a);
            }
            else
            {
                c = Over(c, strandA * 0.7f, a);
                c = Over(c, strandB, b);
            }

            c.a *= fadeEnds;
            return c;
        }

        // A circle; drawn 9-sliced it becomes a pill/rounded rectangle of any size.
        private static Color Pill(Vector2 p, float aa)
        {
            return new Color(1f, 1f, 1f, 1f - SmoothStep(1f - aa * 2f, 1f, p.magnitude));
        }

        // A cute blob with a nucleus and eyes, drawn in greys so Image.color can tint it per Organism.
        private static Color Microbe(Vector2 p, float aa)
        {
            float angle = Mathf.Atan2(p.y, p.x);
            float edge = 0.78f + 0.06f * Mathf.Sin(angle * 5f) + 0.03f * Mathf.Sin(angle * 3f + 1f);
            float d = p.magnitude;

            float body = 1f - SmoothStep(edge - aa, edge + aa, d);
            if (body <= 0f) return Color.clear;

            Color c = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), new Color(0.55f, 0.55f, 0.55f), SmoothStep(0f, edge, d));
            c = Over(c, Color.white, 1f - SmoothStep(0.07f - aa, 0.07f + aa, Mathf.Abs(d - edge + 0.06f)));
            c = Over(c, new Color(0.35f, 0.35f, 0.35f), Disc(p, new Vector2(0.28f, -0.3f), 0.2f, aa));
            c = Over(c, new Color(1f, 1f, 1f), Disc(p, new Vector2(-0.35f, 0.4f), 0.1f, aa) * 0.8f);
            c = Over(c, new Color(0.1f, 0.1f, 0.12f), Disc(p, new Vector2(-0.2f, 0.05f), 0.1f, aa));
            c = Over(c, new Color(0.1f, 0.1f, 0.12f), Disc(p, new Vector2(0.15f, 0.05f), 0.1f, aa));
            c = Over(c, Color.white, Disc(p, new Vector2(-0.17f, 0.09f), 0.035f, aa));
            c = Over(c, Color.white, Disc(p, new Vector2(0.18f, 0.09f), 0.035f, aa));
            c.a = body;
            return c;
        }

        // ---------- Helpers ----------

        private static float Disc(Vector2 p, Vector2 center, float radius, float aa)
        {
            return 1f - SmoothStep(radius - aa, radius + aa, (p - center).magnitude);
        }

        private delegate Color Shader(Vector2 p, float aa);

        private static Sprite Draw(int width, int height, Shader shade, Vector4 border = default)
        {
            var pixels = new Color32[width * height];
            float aa = 2f / Mathf.Min(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = new Vector2((x + 0.5f) / width * 2f - 1f, (y + 0.5f) / height * 2f - 1f);
                    pixels[y * width + x] = shade(p, aa);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static Color Over(Color dst, Color src, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            if (alpha <= 0f) return dst;

            float outA = alpha + dst.a * (1f - alpha);
            Color rgb = (src * alpha + dst * dst.a * (1f - alpha)) / outA;
            rgb.a = outA;
            return rgb;
        }
    }
}
