using CellClicker.Core;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Opens the Upgrades popup; badge while an Upgrade level is affordable.</summary>
    public class UpgradeShopButton : ShopButtonUI
    {
        [SerializeField] private UpgradeSystem upgrades;

        protected override void Subscribe() => upgrades.Changed += Refresh;

        protected override void Unsubscribe() => upgrades.Changed -= Refresh;

        protected override bool AnyAffordable()
        {
            var list = upgrades.Upgrades;
            for (int i = 0; i < list.Count; i++)
            {
                if (upgrades.CanBuy(list[i])) return true;
            }
            return false;
        }
    }
}
