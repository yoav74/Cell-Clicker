using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Pops in, floats up, fades out, then returns itself to the spawner's pool.</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.9f;
        [SerializeField] private float riseDistance = 220f;

        private FloatingTextSpawner owner;
        private RectTransform rect;
        private TMP_Text text;
        private Vector2 start;
        private float age;

        internal void Init(FloatingTextSpawner spawner)
        {
            owner = spawner;
            rect = (RectTransform)transform;
            text = GetComponent<TMP_Text>();
        }

        internal void Play(string message, Vector2 position, Color color)
        {
            text.text = message;
            text.color = color;
            start = position;
            age = 0f;
            gameObject.SetActive(true);
            Apply(0f);
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = age / lifetime;

            if (t >= 1f)
            {
                owner.Release(this);
                return;
            }

            Apply(t);
        }

        private void Apply(float t)
        {
            float easeOut = 1f - (1f - t) * (1f - t);
            rect.anchoredPosition = start + new Vector2(0f, riseDistance * easeOut);

            // Quick pop: grow past full size, then settle.
            float pop = t < 0.12f ? Mathf.Lerp(0.5f, 1.2f, t / 0.12f) : Mathf.Lerp(1.2f, 1f, Mathf.Clamp01((t - 0.12f) / 0.15f));
            rect.localScale = Vector3.one * pop;

            // Fade via the CanvasRenderer: no text mesh rebuild every frame.
            text.canvasRenderer.SetAlpha(1f - t * t);
        }
    }
}
