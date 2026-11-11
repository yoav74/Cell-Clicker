using CellClicker.Core;
using CellClicker.Organisms;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>One Organism in the shop: Organism Level, DNA/sec and the Recruit / Level Up button.</summary>
    public class OrganismRowUI : ShopRowUI
    {
        [SerializeField] private Color lockedIconColor = new Color(0f, 0f, 0f, 0.55f);

        [Header("Text")]
        [SerializeField] private string recruitLabel = "Recruit";
        [SerializeField] private string levelUpLabel = "Level Up";
        [SerializeField] private string lockedLabel = "Locked";
        [SerializeField] private string lockedName = "???";

        private Organism organism;
        private OrganismRoster roster;
        private DnaBank bank;

        protected override bool CanBuy => roster.CanLevelUp(organism);

        public void Bind(Organism target, OrganismRoster owner, DnaBank dnaBank)
        {
            organism = target;
            roster = owner;
            bank = dnaBank;
            ListenToBuyButton();
            SetIconArt(target.Definition.Icon);
        }

        private void OnEnable()
        {
            if (organism == null) return;

            bank.Changed += OnDnaChanged;
            roster.LevelChanged += OnLevelChanged;
            RefreshAll();
        }

        private void OnDisable()
        {
            if (organism == null) return;

            bank.Changed -= OnDnaChanged;
            roster.LevelChanged -= OnLevelChanged;
        }

        protected override bool TryBuy() => roster.TryLevelUp(organism);

        // Any Organism leveling up can unlock this one, so refresh every row.
        private void OnLevelChanged(Organism changed) => RefreshAll();

        private void OnDnaChanged(double total) => RefreshButton();

        protected override void RefreshTexts()
        {
            OrganismDefinition definition = organism.Definition;
            bool unlocked = roster.IsUnlocked(organism);

            icon.color = unlocked ? definition.Tint : lockedIconColor;
            nameText.text = unlocked ? definition.DisplayName : lockedName;
            infoText.text = InfoLine(unlocked);
            actionText.text = !unlocked ? lockedLabel : organism.IsRecruited ? levelUpLabel : recruitLabel;
            costText.text = NumberFormatter.Format(organism.NextCost);
        }

        private string InfoLine(bool unlocked)
        {
            if (!unlocked) return "Recruit the one above to unlock";
            if (!organism.IsRecruited) return "+" + NumberFormatter.FormatRate(organism.NextLevelGain) + " DNA/sec";

            return "Lv " + organism.Level + "   " + NumberFormatter.FormatRate(organism.DnaPerSecond) + " DNA/sec";
        }
    }
}
