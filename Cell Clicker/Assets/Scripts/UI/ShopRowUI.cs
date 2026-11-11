using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// One row in the shop: icon, name, info line and a buy button that punches on purchase.
    /// Subclasses say what is bought; texts are only rebuilt on <see cref="RefreshAll"/>, and the
    /// per-frame DNA updates only call <see cref="RefreshButton"/>, which flips the button when needed.
    /// </summary>
    public abstract class ShopRowUI : MonoBehaviour
    {
        [SerializeField] protected Image icon;
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text infoText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image buyBackground;
        [SerializeField] protected TMP_Text actionText;
        [SerializeField] protected TMP_Text costText;
        [Tooltip("Scaled for a quick bounce after buying.")]
        [SerializeField] private RectTransform punchTarget;

        [Header("Look")]
        [SerializeField] private Color affordableColor = new Color32(76, 196, 120, 255);
        [SerializeField] private Color unaffordableColor = new Color32(58, 74, 86, 255);
        [SerializeField] private float punchScale = 1.15f;
        [SerializeField] private float punchRecoverSpeed = 10f;

        private bool? shownAffordable;
        private float punch;
        private bool listening;

        protected abstract bool CanBuy { get; }

        protected abstract bool TryBuy();

        /// <summary>Rebuild the icon tint and all texts.</summary>
        protected abstract void RefreshTexts();

        protected virtual void Update()
        {
            if (punch <= 0f) return;

            punch = Mathf.MoveTowards(punch, 0f, punchRecoverSpeed * Time.unscaledDeltaTime);
            punchTarget.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, punch);
        }

        /// <summary>Call once the row has its data (from Bind or Awake).</summary>
        protected void ListenToBuyButton()
        {
            if (listening) return;

            listening = true;
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        /// <summary>Swaps the code-drawn placeholder for real art, if there is any.</summary>
        protected void SetIconArt(Sprite art)
        {
            if (art == null) return;

            if (icon.TryGetComponent(out ProceduralImage placeholder)) placeholder.enabled = false;
            icon.overrideSprite = null;
            icon.sprite = art;
        }

        protected void RefreshAll()
        {
            RefreshTexts();
            shownAffordable = null;
            RefreshButton();
        }

        protected void RefreshButton()
        {
            bool affordable = CanBuy;
            if (affordable == shownAffordable) return;

            shownAffordable = affordable;
            buyButton.interactable = affordable;
            buyBackground.color = affordable ? affordableColor : unaffordableColor;
        }

        private void OnBuyClicked()
        {
            if (TryBuy()) punch = 1f;
        }
    }
}
