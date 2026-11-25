using CellClicker.Core;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>One Upgrade in the Upgrades popup: its level, what it does and the Buy button.</summary>
    public class UpgradeRowUI : ShopRowUI
    {
        [Header("Text")]
        [SerializeField] private string buyLabel = "Buy";
        [SerializeField] private string upgradeLabel = "Upgrade";
        [SerializeField] private string maxedLabel = "MAX";

        private Upgrade upgrade;
        private UpgradeSystem system;
        private DnaBank bank;

        protected override bool CanBuy => system.CanBuy(upgrade);

        public void Bind(Upgrade target, UpgradeSystem owner, DnaBank dnaBank)
        {
            upgrade = target;
            system = owner;
            bank = dnaBank;
            ListenToBuyButton();
            SetIconArt(target.Definition.Icon);
        }

        private void OnEnable()
        {
            if (upgrade == null) return;

            bank.Changed += OnDnaChanged;
            system.Changed += RefreshAll;
            RefreshAll();
        }

        private void OnDisable()
        {
            if (upgrade == null) return;

            bank.Changed -= OnDnaChanged;
            system.Changed -= RefreshAll;
        }

        protected override bool TryBuy() => system.TryBuy(upgrade);

        private void OnDnaChanged(double total) => RefreshButton();

        protected override void RefreshTexts()
        {
            UpgradeDefinition definition = upgrade.Definition;

            icon.color = definition.Tint;
            nameText.text = definition.DisplayName;
            infoText.text = InfoLine();
            actionText.text = upgrade.IsMaxed ? maxedLabel : upgrade.Level == 0 ? buyLabel : upgradeLabel;
            costText.text = upgrade.IsMaxed ? "-" : NumberFormatter.Format(upgrade.NextCost);
        }

        // Before the first level, sell what it does; after, show the level and the total effect so far.
        private string InfoLine()
        {
            UpgradeDefinition definition = upgrade.Definition;
            if (upgrade.Level == 0) return definition.DescribeNextLevel();

            return "Lv " + upgrade.Level + "/" + definition.MaxLevel + "   " + definition.DescribeTotal(upgrade.Level);
        }
    }
}
