using CellClicker.Hive;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// On every tap: an expanding ring plus a burst of colored dots at the finger.
    /// All pieces are created once and recycled, and one Update moves them all (cheap on phones).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TapBurstFeedback : HiveTapFeedback
    {
        [Header("Particles")]
        [SerializeField] private int particlesPerTap = 8;
        [SerializeField] private int maxParticles = 64;
        [SerializeField] private Vector2 speedRange = new Vector2(500f, 1100f);
        [SerializeField] private Vector2 sizeRange = new Vector2(16f, 34f);
        [SerializeField] private float particleLifetime = 0.55f;
        [SerializeField] private float drag = 4f;

        [Header("Ring")]
        [SerializeField] private int maxRings = 6;
        [SerializeField] private float ringLifetime = 0.35f;
        [SerializeField] private float ringSize = 280f;

        [SerializeField] private Color[] palette =
        {
            new Color32(110, 235, 195, 255),
            new Color32(242, 193, 78, 255),
            new Color32(170, 125, 255, 255),
        };

        private struct Piece
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public Color Color;
            public float Size;
            public float Age;
            public bool Alive;
        }

        private RectTransform layer;
        private Piece[] particles;
        private Piece[] rings;
        private int nextParticle;
        private int nextRing;

        private void Awake()
        {
            layer = (RectTransform)transform;
            particles = CreatePool("Burst Dot", maxParticles, ProceduralSpriteKind.SoftDot);
            rings = CreatePool("Burst Ring", maxRings, ProceduralSpriteKind.Ring);
        }

        protected override void OnHiveTapped(HiveTap tap)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, tap.ScreenPosition, tap.EventCamera, out Vector2 local);

            Launch(ref rings[nextRing], local, Vector2.zero, Color.white, ringSize);
            nextRing = (nextRing + 1) % rings.Length;

            for (int i = 0; i < particlesPerTap; i++)
            {
                Vector2 direction = Random.insideUnitCircle.normalized;
                Vector2 velocity = direction * Random.Range(speedRange.x, speedRange.y);
                Color color = palette[Random.Range(0, palette.Length)];
                Launch(ref particles[nextParticle], local, velocity, color, Random.Range(sizeRange.x, sizeRange.y));
                nextParticle = (nextParticle + 1) % particles.Length; // oldest piece gets reused when full
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float damping = Mathf.Exp(-drag * dt);

            for (int i = 0; i < particles.Length; i++)
            {
                ref Piece p = ref particles[i];
                if (!p.Alive) continue;

                p.Age += dt;
                float t = p.Age / particleLifetime;
                if (t >= 1f) { Kill(ref p); continue; }

                p.Velocity *= damping;
                p.Rect.anchoredPosition += p.Velocity * dt;
                p.Rect.localScale = Vector3.one * (1f - t * 0.6f);
                p.Image.color = WithAlpha(p.Color, 1f - t * t);
            }

            for (int i = 0; i < rings.Length; i++)
            {
                ref Piece r = ref rings[i];
                if (!r.Alive) continue;

                r.Age += dt;
                float t = r.Age / ringLifetime;
                if (t >= 1f) { Kill(ref r); continue; }

                float easeOut = 1f - (1f - t) * (1f - t);
                r.Rect.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, easeOut);
                r.Image.color = WithAlpha(r.Color, (1f - t) * 0.7f);
            }
        }

        private Piece[] CreatePool(string name, int count, ProceduralSpriteKind sprite)
        {
            var pool = new Piece[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(name, typeof(RectTransform));
                var rect = (RectTransform)go.transform;
                rect.SetParent(layer, false);

                var image = go.AddComponent<Image>();
                image.sprite = ProceduralSprites.Get(sprite);
                image.raycastTarget = false;

                go.SetActive(false);
                pool[i] = new Piece { Rect = rect, Image = image };
            }
            return pool;
        }

        private static void Launch(ref Piece piece, Vector2 position, Vector2 velocity, Color color, float size)
        {
            piece.Rect.anchoredPosition = position;
            piece.Rect.sizeDelta = new Vector2(size, size);
            piece.Rect.localScale = Vector3.one;
            piece.Velocity = velocity;
            piece.Color = color;
            piece.Size = size;
            piece.Age = 0f;
            piece.Alive = true;
            piece.Image.color = color;
            piece.Rect.gameObject.SetActive(true);
        }

        private static void Kill(ref Piece piece)
        {
            piece.Alive = false;
            piece.Rect.gameObject.SetActive(false);
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
