using CellClicker.Core;
using CellClicker.Organisms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// One Organism in the shop: icon, name, Organism Level, DNA/sec and the buy button.
    /// Texts are only rebuilt when the Organism changes; the per-frame DNA updates only flip the button state.
    /// </summary>
    public class OrganismRowUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image buyBackground;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_Text costText;
        [Tooltip("Scaled for a quick bounce after buying.")]
        [SerializeField] private RectTransform punchTarget;

        [Header("Look")]
        [SerializeField] private Color affordableColor = new Color32(76, 196, 120, 255);
        [SerializeField] private Color unaffordableColor = new Color32(58, 74, 86, 255);
        [SerializeField] private Color lockedIconColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private float punchScale = 1.15f;
        [SerializeField] private float punchRecoverSpeed = 10f;

        [Header("Text")]
        [SerializeField] private string recruitLabel = "Recruit";
        [SerializeField] private string levelUpLabel = "Level Up";
        [SerializeField] private string lockedLabel = "Locked";
        [SerializeField] private string lockedName = "???";

        private Organism organism;
        private OrganismRoster roster;
        private DnaBank bank;
        private bool? shownAffordable;
        private float punch;

        public void Bind(Organism target, OrganismRoster owner, DnaBank dnaBank)
        {
            organism = target;
            roster = owner;
            bank = dnaBank;
            buyButton.onClick.AddListener(OnBuyClicked);

            Sprite art = target.Definition.Icon;
            if (art == null) return;

            // Real art: switch off the code-drawn placeholder before the row is first enabled.
            if (icon.TryGetComponent(out ProceduralImage placeholder)) placeholder.enabled = false;
            icon.overrideSprite = null;
            icon.sprite = art;
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

        private void Update()
        {
            if (punch <= 0f) return;

            punch = Mathf.MoveTowards(punch, 0f, punchRecoverSpeed * Time.unscaledDeltaTime);
            punchTarget.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, punch);
        }

        private void OnBuyClicked()
        {
            if (roster.TryLevelUp(organism)) punch = 1f;
        }

        // Any Organism leveling up can unlock this one, so refresh every row.
        private void OnLevelChanged(Organism changed) => RefreshAll();

        private void OnDnaChanged(double total) => RefreshButton();

        private void RefreshAll()
        {
            OrganismDefinition definition = organism.Definition;
            bool unlocked = roster.IsUnlocked(organism);

            icon.color = unlocked ? definition.Tint : lockedIconColor;
            nameText.text = unlocked ? definition.DisplayName : lockedName;
            infoText.text = InfoLine(unlocked);
            actionText.text = !unlocked ? lockedLabel : organism.IsRecruited ? levelUpLabel : recruitLabel;
            costText.text = NumberFormatter.Format(organism.NextCost);

            shownAffordable = null;
            RefreshButton();
        }

        private string InfoLine(bool unlocked)
        {
            if (!unlocked) return "Recruit the one above to unlock";
            if (!organism.IsRecruited) return "+" + NumberFormatter.FormatRate(organism.NextLevelGain) + " DNA/sec";

            return "Lv " + organism.Level + "   " + NumberFormatter.FormatRate(organism.DnaPerSecond) + " DNA/sec";
        }

        private void RefreshButton()
        {
            bool affordable = roster.CanLevelUp(organism);
            if (affordable == shownAffordable) return;

            shownAffordable = affordable;
            buyButton.interactable = affordable;
            buyBackground.color = affordable ? affordableColor : unaffordableColor;
        }
    }
}
