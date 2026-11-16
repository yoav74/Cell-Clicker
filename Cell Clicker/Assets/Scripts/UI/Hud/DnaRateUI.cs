using CellClicker.Core;
using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Shows the total DNA/sec. Only rebuilds the text when the rate changes.</summary>
    public class DnaRateUI : MonoBehaviour
    {
        [SerializeField] private DnaGenerator generator;
        [SerializeField] private TMP_Text rateText;
        [SerializeField] private string suffix = " DNA/sec";

        private void OnEnable()
        {
            generator.RateChanged += Show;
            Show(generator.DnaPerSecond);
        }

        private void OnDisable() => generator.RateChanged -= Show;

        private void Show(double rate) => rateText.text = NumberFormatter.FormatRate(rate) + suffix;
    }
}
