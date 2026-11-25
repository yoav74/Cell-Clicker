using CellClicker.Core;
using CellClicker.Hive;
using CellClicker.Organisms;
using CellClicker.UI;
using UnityEngine;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Builds the Organisms button (bottom-left, below the play area) and the Organisms popup
    /// (Tap Power first, then the Organisms). Called by <see cref="HiveSceneBuilder"/>.
    /// </summary>
    public static class OrganismShopBuilder
    {
        private static readonly ShopPopupBuilder.ButtonStyle Style = new ShopPopupBuilder.ButtonStyle(
            "Organisms", ProceduralSpriteKind.Microbe,
            accent: new Color32(110, 235, 195, 255), fill: new Color32(22, 74, 78, 255), iconColor: new Color32(150, 240, 205, 255));

        public static void Build(Transform canvas, Transform safeArea, DnaBank bank, TapPower tapPower, OrganismRoster roster)
        {
            PopupPanel popup = ShopPopupBuilder.BuildPopup(canvas, "Organisms Popup", "Organisms",
                "Boost your taps and recruit Organisms", out RectTransform content);

            // Template first (hidden at runtime), then Tap Power, so the cloned Organism rows land below it.
            OrganismRowUI rowTemplate = ShopPopupBuilder.BuildRow<OrganismRowUI>(content, "Row Template", ProceduralSpriteKind.Microbe);
            TapPowerRowUI tapRow = ShopPopupBuilder.BuildRow<TapPowerRowUI>(content, "Tap Power Row", ProceduralSpriteKind.Cell);
            Wire(tapRow, "tapPower", tapPower);
            Wire(tapRow, "bank", bank);

            var shop = popup.gameObject.AddComponent<OrganismShopUI>();
            Wire(shop, "roster", roster);
            Wire(shop, "bank", bank);
            Wire(shop, "rowTemplate", rowTemplate);
            Wire(shop, "content", content);

            var button = ShopPopupBuilder.BuildButton<OrganismShopButton>(safeArea, "Organisms Button", Style, right: false, bank, popup);
            Wire(button, "roster", roster);
            Wire(button, "tapPower", tapPower);
        }
    }
}
