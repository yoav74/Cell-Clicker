using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>
    /// Draws a placeholder "cell hive" sprite in code (membrane, honeycomb cytoplasm, nucleus, organelles)
    /// so the game has a look before real art exists. Replace with a real sprite later.
    /// </summary>
    public static class CellSpriteFactory
    {
        private static readonly Color Glow = new Color32(92, 225, 181, 255);
        private static readonly Color Membrane = new Color32(110, 235, 195, 255);
        private static readonly Color CytoplasmInner = new Color32(64, 178, 145, 240);
        private static readonly Color CytoplasmOuter = new Color32(26, 102, 96, 240);
        private static readonly Color HoneycombLine = new Color32(160, 255, 220, 255);
        private static readonly Color Nucleus = new Color32(150, 105, 245, 255);
        private static readonly Color NucleusEdge = new Color32(95, 60, 190, 255);
        private static readonly Color Nucleolus = new Color32(70, 40, 150, 255);
        private static readonly Color Organelle = new Color32(242, 193, 78, 255);

        private const float CellRadius = 0.84f;      // in [-1, 1] sprite space
        private const float MembraneWidth = 0.035f;
        private const float HoneycombSize = 0.2f;

        private static readonly Vector2 NucleusCenter = new Vector2(0.1f, 0.06f);
        private const float NucleusRadius = 0.28f;

        // x, y, radius
        private static readonly Vector3[] Organelles =
        {
            new Vector3(-0.45f, -0.25f, 0.06f),
            new Vector3(-0.30f, 0.45f, 0.045f),
            new Vector3(0.50f, -0.38f, 0.05f),
            new Vector3(-0.10f, -0.55f, 0.04f),
            new Vector3(0.55f, 0.35f, 0.035f),
        };

        public static Sprite Create(int size = 512)
        {
            var pixels = new Color32[size * size];
            float aa = 2f / size; // one pixel in [-1, 1] space, used for anti-aliasing

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    pixels[y * size + x] = Shade(p, aa);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "GeneratedCellHive",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true); // true = drop the CPU copy, halves the texture's memory

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Color Shade(Vector2 p, float aa)
        {
            // Wobble the radius a little so the membrane looks organic, not a perfect circle.
            float angle = Mathf.Atan2(p.y, p.x);
            float d = p.magnitude + 0.012f * Mathf.Sin(angle * 6f) + 0.008f * Mathf.Sin(angle * 11f + 1f);

            Color c = Color.clear;

            // Soft outer glow.
            if (d > CellRadius - MembraneWidth)
            {
                float glow = 1f - SmoothStep(CellRadius, 1f, d);
                c = Over(c, Glow, glow * 0.35f);
            }

            float inside = 1f - SmoothStep(CellRadius - aa, CellRadius + aa, d);

            // Cytoplasm, lighter toward the center.
            Color cyto = Color.Lerp(CytoplasmInner, CytoplasmOuter, Mathf.Clamp01(d / CellRadius));
            c = Over(c, cyto, inside * cyto.a);

            // Honeycomb pattern: this is the "hive".
            float hex = HexEdgeDistance(p / HoneycombSize);
            float hexAa = aa / HoneycombSize;
            float line = SmoothStep(0.45f - hexAa, 0.45f, hex);
            c = Over(c, HoneycombLine, line * inside * 0.22f);

            // Organelles.
            foreach (var o in Organelles)
            {
                float od = Vector2.Distance(p, new Vector2(o.x, o.y));
                c = Over(c, Organelle, 1f - SmoothStep(o.z - aa, o.z + aa, od));
            }

            // Nucleus with a darker rim and a nucleolus.
            float nd = Vector2.Distance(p, NucleusCenter);
            Color nucleus = Color.Lerp(Nucleus, NucleusEdge, SmoothStep(NucleusRadius * 0.6f, NucleusRadius, nd));
            c = Over(c, nucleus, 1f - SmoothStep(NucleusRadius - aa, NucleusRadius + aa, nd));

            float nld = Vector2.Distance(p, NucleusCenter + new Vector2(0.06f, 0.05f));
            c = Over(c, Nucleolus, 1f - SmoothStep(0.09f - aa, 0.09f + aa, nld));

            // Membrane ring, lit from the top-left.
            float ring = 1f - SmoothStep(MembraneWidth - aa, MembraneWidth + aa, Mathf.Abs(d - (CellRadius - MembraneWidth)));
            float light = Mathf.Max(0f, Vector2.Dot(p.normalized, new Vector2(-0.6f, 0.8f)));
            c = Over(c, Color.Lerp(Membrane, Color.white, light * 0.45f), ring);

            // Glossy highlight.
            float hd = Vector2.Distance(p, new Vector2(-0.35f, 0.42f));
            c = Over(c, Color.white, (1f - SmoothStep(0f, 0.28f, hd)) * 0.18f * inside);

            return c;
        }

        /// <summary>Distance from the center of the nearest hexagon: 0 at the center, 0.5 on its edge.</summary>
        private static float HexEdgeDistance(Vector2 p)
        {
            var r = new Vector2(1f, 1.7320508f);
            var h = r * 0.5f;
            Vector2 a = Mod(p, r) - h;
            Vector2 b = Mod(p - h, r) - h;
            Vector2 g = a.sqrMagnitude < b.sqrMagnitude ? a : b;
            g = new Vector2(Mathf.Abs(g.x), Mathf.Abs(g.y));
            return Mathf.Max(g.x, g.x * 0.5f + g.y * 0.8660254f);
        }

        private static Vector2 Mod(Vector2 v, Vector2 m) =>
            new Vector2(v.x - m.x * Mathf.Floor(v.x / m.x), v.y - m.y * Mathf.Floor(v.y / m.y));

        // GLSL-style smoothstep (Mathf.SmoothStep is a different function).
        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        // Paints src over dst with the given opacity.
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
