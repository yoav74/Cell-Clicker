using CellClicker.Core;
using CellClicker.UI;
using UnityEngine;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Builds the Upgrades button (bottom-right, mirroring Organisms) and the Upgrades popup, one row per
    /// Upgrade in the Economy Config. Called by <see cref="HiveSceneBuilder"/>.
    /// </summary>
    public static class UpgradeShopBuilder
    {
        private static readonly ShopPopupBuilder.ButtonStyle Style = new ShopPopupBuilder.ButtonStyle(
            "Upgrades", ProceduralSpriteKind.Helix,
            accent: new Color32(205, 150, 255, 255), fill: new Color32(58, 34, 86, 255), iconColor: new Color32(225, 195, 255, 255));

        public static void Build(Transform canvas, Transform safeArea, DnaBank bank, UpgradeSystem upgrades)
        {
            PopupPanel popup = ShopPopupBuilder.BuildPopup(canvas, "Upgrades Popup", "Upgrades",
                "Permanent boosts for all your DNA", out RectTransform content);

            UpgradeRowUI rowTemplate = ShopPopupBuilder.BuildRow<UpgradeRowUI>(content, "Row Template", ProceduralSpriteKind.Helix);

            var shop = popup.gameObject.AddComponent<UpgradeShopUI>();
            Wire(shop, "upgrades", upgrades);
            Wire(shop, "bank", bank);
            Wire(shop, "rowTemplate", rowTemplate);
            Wire(shop, "content", content);

            var button = ShopPopupBuilder.BuildButton<UpgradeShopButton>(safeArea, "Upgrades Button", Style, right: true, bank, popup);
            Wire(button, "upgrades", upgrades);
        }
    }
}
