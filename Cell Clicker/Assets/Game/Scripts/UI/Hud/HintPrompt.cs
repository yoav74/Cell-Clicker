using CellClicker.Core;
using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Pulses "Tap the hive" until the player earns their first DNA, then fades away.</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HintPrompt : MonoBehaviour
    {
        [SerializeField] private DnaBank bank;
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private Vector2 pulseAlpha = new Vector2(0.35f, 1f);
        [SerializeField] private float fadeOutTime = 0.5f;

        private CanvasRenderer canvasRenderer;
        private float fade = 1f;
        private bool dismissed;

        private void Awake()
        {
            // Fading through the CanvasRenderer doesn't rebuild the text mesh every frame.
            canvasRenderer = GetComponent<TMP_Text>().canvasRenderer;
        }

        private void OnEnable() => bank.Changed += OnDnaChanged;
        private void OnDisable() => bank.Changed -= OnDnaChanged;

        private void OnDnaChanged(double total)
        {
            dismissed = true;
            bank.Changed -= OnDnaChanged;
        }

        private void Update()
        {
            if (dismissed)
            {
                fade -= Time.deltaTime / fadeOutTime;
                if (fade <= 0f)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
            canvasRenderer.SetAlpha(Mathf.Lerp(pulseAlpha.x, pulseAlpha.y, pulse) * fade);
        }
    }
}
