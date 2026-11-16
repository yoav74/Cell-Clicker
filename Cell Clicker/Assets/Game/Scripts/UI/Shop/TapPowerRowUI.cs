using CellClicker.Core;
using CellClicker.Hive;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>The Tap Power row at the top of the shop: its level, DNA per tap and the Upgrade button.</summary>
    public class TapPowerRowUI : ShopRowUI
    {
        [SerializeField] private TapPower tapPower;
        [SerializeField] private DnaBank bank;

        [Header("Text")]
        [SerializeField] private string upgradeLabel = "Upgrade";

        protected override bool CanBuy => tapPower.CanLevelUp;

        private void Awake()
        {
            ListenToBuyButton();
            SetIconArt(tapPower.Definition.Icon);
        }

        private void OnEnable()
        {
            bank.Changed += OnDnaChanged;
            tapPower.Changed += RefreshAll;
            RefreshAll();
        }

        private void OnDisable()
        {
            bank.Changed -= OnDnaChanged;
            tapPower.Changed -= RefreshAll;
        }

        protected override bool TryBuy() => tapPower.TryLevelUp();

        private void OnDnaChanged(double total) => RefreshButton();

        protected override void RefreshTexts()
        {
            icon.color = tapPower.Definition.Tint;
            nameText.text = tapPower.Definition.DisplayName;
            infoText.text = InfoLine();
            actionText.text = upgradeLabel;
            costText.text = NumberFormatter.Format(tapPower.NextCost);
        }

        // Before the first upgrade, show what it adds; after, show the current level and DNA per tap.
        private string InfoLine()
        {
            if (tapPower.Level == 0) return "+" + NumberFormatter.FormatRate(tapPower.NextLevelGain) + " DNA per tap";

            return "Lv " + tapPower.Level + "   " + NumberFormatter.FormatRate(tapPower.Value) + " DNA per tap";
        }
    }
}
