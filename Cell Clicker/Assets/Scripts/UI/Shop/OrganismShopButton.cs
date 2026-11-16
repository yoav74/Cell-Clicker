using CellClicker.Core;
using CellClicker.Hive;
using CellClicker.Organisms;
using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// Opens the Organisms popup. Shows a badge and gently pulses while the player can afford something
    /// (Tap Power or an Organism),
    /// so the next goal is visible without distracting from tapping the hive.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class OrganismShopButton : MonoBehaviour
    {
        [SerializeField] private OrganismRoster roster;
        [SerializeField] private TapPower tapPower;
        [SerializeField] private DnaBank bank;
        [SerializeField] private PopupPanel popup;
        [SerializeField] private GameObject badge;
        [SerializeField] private RectTransform pulseTarget;
        [SerializeField] private float pulseSpeed = 1.6f;
        [SerializeField] private float pulseScale = 1.07f;

        private bool canBuy;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(popup.Open);
            badge.SetActive(false);
        }

        private void OnEnable()
        {
            bank.Changed += OnDnaChanged;
            roster.LevelChanged += OnLevelChanged;
            tapPower.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            bank.Changed -= OnDnaChanged;
            roster.LevelChanged -= OnLevelChanged;
            tapPower.Changed -= Refresh;
        }

        private void Update()
        {
            if (!canBuy) return;

            float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f);
            pulseTarget.localScale = Vector3.one * Mathf.Lerp(1f, pulseScale, wave);
        }

        private void OnDnaChanged(double total) => Refresh();
        private void OnLevelChanged(Organism organism) => Refresh();

        private void Refresh()
        {
            // Runs every frame while DNA/sec flows in: indexed loop, because foreach over an interface allocates.
            bool any = tapPower.CanLevelUp;
            var organisms = roster.Organisms;
            for (int i = 0; i < organisms.Count && !any; i++)
            {
                any = roster.CanLevelUp(organisms[i]);
            }

            if (any == canBuy) return;

            canBuy = any;
            badge.SetActive(any);
            if (!any) pulseTarget.localScale = Vector3.one;
        }
    }
}
