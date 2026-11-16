using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// Soft glowing specks drifting up the background, like particles under a microscope.
    /// Keeps the screen alive even when the player isn't tapping.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BackgroundMotes : MonoBehaviour
    {
        [SerializeField] private int count = 24;
        [SerializeField] private Vector2 sizeRange = new Vector2(10f, 40f);
        [SerializeField] private Vector2 speedRange = new Vector2(15f, 60f);
        [SerializeField] private Vector2 alphaRange = new Vector2(0.05f, 0.25f);
        [SerializeField] private float swayAmount = 25f;
        [SerializeField] private Color[] colors =
        {
            new Color32(110, 235, 195, 255),
            new Color32(170, 125, 255, 255),
        };

        private struct Mote
        {
            public RectTransform Rect;
            public float BaseX;
            public float Speed;
            public float Phase;
        }

        private RectTransform area;
        private Mote[] motes;

        // Start, not Awake: the canvas layout (and so our size) is ready by then.
        private void Start()
        {
            area = (RectTransform)transform;
            motes = new Mote[count];
            Rect bounds = area.rect;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Mote", typeof(RectTransform));
                var rect = (RectTransform)go.transform;
                rect.SetParent(area, false);
                float size = Random.Range(sizeRange.x, sizeRange.y);
                rect.sizeDelta = new Vector2(size, size);

                var image = go.AddComponent<Image>();
                image.sprite = ProceduralSprites.Get(ProceduralSpriteKind.SoftDot);
                image.raycastTarget = false;
                Color c = colors[Random.Range(0, colors.Length)];
                c.a = Random.Range(alphaRange.x, alphaRange.y);
                image.color = c;

                motes[i] = new Mote
                {
                    Rect = rect,
                    BaseX = Random.Range(bounds.xMin, bounds.xMax),
                    Speed = Random.Range(speedRange.x, speedRange.y),
                    Phase = Random.Range(0f, Mathf.PI * 2f),
                };
                rect.anchoredPosition = new Vector2(motes[i].BaseX, Random.Range(bounds.yMin, bounds.yMax));
            }
        }

        private void Update()
        {
            if (motes == null) return;

            Rect bounds = area.rect;
            float dt = Time.deltaTime;
            float time = Time.time;

            for (int i = 0; i < motes.Length; i++)
            {
                ref Mote m = ref motes[i];
                Vector2 pos = m.Rect.anchoredPosition;
                pos.y += m.Speed * dt;

                // Wrap around to the bottom once it leaves the top.
                if (pos.y > bounds.yMax + 50f)
                {
                    pos.y = bounds.yMin - 50f;
                    m.BaseX = Random.Range(bounds.xMin, bounds.xMax);
                }

                pos.x = m.BaseX + Mathf.Sin(time * 0.5f + m.Phase) * swayAmount;
                m.Rect.anchoredPosition = pos;
            }
        }
    }
}
