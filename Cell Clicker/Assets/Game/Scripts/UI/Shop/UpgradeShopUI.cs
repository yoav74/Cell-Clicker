using CellClicker.Core;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Fills the Upgrades popup with one row per Upgrade, once, from a template row.</summary>
    public class UpgradeShopUI : MonoBehaviour
    {
        [SerializeField] private UpgradeSystem upgrades;
        [SerializeField] private DnaBank bank;
        [SerializeField] private UpgradeRowUI rowTemplate;
        [SerializeField] private RectTransform content;

        private void Awake()
        {
            rowTemplate.gameObject.SetActive(false);

            foreach (Upgrade upgrade in upgrades.Upgrades)
            {
                UpgradeRowUI row = Instantiate(rowTemplate, content);
                row.name = upgrade.Definition.DisplayName;
                row.Bind(upgrade, upgrades, bank);
                row.gameObject.SetActive(true);
            }
        }
    }
}
