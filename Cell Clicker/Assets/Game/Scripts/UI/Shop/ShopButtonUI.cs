using CellClicker.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// A button that opens a shop popup. Shows a badge and gently pulses while the player can afford something
    /// in that shop, so the next goal is visible without distracting from tapping the hive.
    /// Subclasses say what "something affordable" means and which events can change it.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class ShopButtonUI : MonoBehaviour
    {
        [SerializeField] protected DnaBank bank;
        [SerializeField] private PopupPanel popup;
        [SerializeField] private GameObject badge;
        [SerializeField] private RectTransform pulseTarget;
        [SerializeField] private float pulseSpeed = 1.6f;
        [SerializeField] private float pulseScale = 1.07f;

        private bool canBuy;

        /// <summary>Runs every frame while DNA/sec flows in, so it must not allocate.</summary>
        protected abstract bool AnyAffordable();

        /// <summary>Subscribe to the events (besides DNA changes) that can change <see cref="AnyAffordable"/>.</summary>
        protected abstract void Subscribe();

        protected abstract void Unsubscribe();

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(popup.Open);
            badge.SetActive(false);
        }

        private void OnEnable()
        {
            bank.Changed += OnDnaChanged;
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            bank.Changed -= OnDnaChanged;
            Unsubscribe();
        }

        private void Update()
        {
            if (!canBuy) return;

            float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f);
            pulseTarget.localScale = Vector3.one * Mathf.Lerp(1f, pulseScale, wave);
        }

        private void OnDnaChanged(double total) => Refresh();

        protected void Refresh()
        {
            bool any = AnyAffordable();
            if (any == canBuy) return;

            canBuy = any;
            badge.SetActive(any);
            if (!any) pulseTarget.localScale = Vector3.one;
        }
    }
}
