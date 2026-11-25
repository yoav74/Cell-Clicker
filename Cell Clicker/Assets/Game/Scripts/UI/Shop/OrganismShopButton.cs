using CellClicker.Hive;
using CellClicker.Organisms;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Opens the Organisms popup; badge while Tap Power or an Organism is affordable.</summary>
    public class OrganismShopButton : ShopButtonUI
    {
        [SerializeField] private OrganismRoster roster;
        [SerializeField] private TapPower tapPower;

        protected override void Subscribe()
        {
            roster.LevelChanged += OnLevelChanged;
            roster.CostsChanged += Refresh;
            tapPower.Changed += Refresh;
        }

        protected override void Unsubscribe()
        {
            roster.LevelChanged -= OnLevelChanged;
            roster.CostsChanged -= Refresh;
            tapPower.Changed -= Refresh;
        }

        protected override bool AnyAffordable()
        {
            // Indexed loop: foreach over an interface allocates, and this runs every frame.
            if (tapPower.CanLevelUp) return true;

            var organisms = roster.Organisms;
            for (int i = 0; i < organisms.Count; i++)
            {
                if (roster.CanLevelUp(organisms[i])) return true;
            }
            return false;
        }

        private void OnLevelChanged(Organism organism) => Refresh();
    }
}
