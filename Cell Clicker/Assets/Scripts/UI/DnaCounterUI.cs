using System;
using CellClicker.Core;
using CellClicker.Hive;
using TMPro;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>
    /// Shows the DNA total and Tap Power. The total rolls toward the real value,
    /// and flashes and bumps on every tap (not on DNA/sec income, which arrives every frame).
    /// </summary>
    public class DnaCounterUI : MonoBehaviour
    {
        [SerializeField] private DnaBank bank;
        [SerializeField] private DnaHive hive;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text perTapText;

        [Header("Juice")]
        [SerializeField] private float rollSpeed = 12f;
        [SerializeField] private float bumpScale = 1.12f;
        [SerializeField] private float bumpRecoverSpeed = 12f;
        [SerializeField] private Color flashColor = new Color32(255, 235, 140, 255);

        private const float BumpEpsilon = 0.001f;

        private double shown;
        private double shownFloor = -1;
        private float bump;
        private Color baseColor;

        private void OnEnable() => hive.Tapped += OnHiveTapped;
        private void OnDisable() => hive.Tapped -= OnHiveTapped;

        private void Start()
        {
            baseColor = amountText.color;
            shown = bank.Amount;
            RefreshAmountText();
            perTapText.text = "+" + NumberFormatter.Format(hive.DnaPerTap) + " per tap";
        }

        private void OnHiveTapped(HiveTap tap) => bump = 1f;

        private void Update()
        {
            float dt = Time.deltaTime;
            RollTowards(bank.Amount, dt);
            AnimateBump(dt);
        }

        private void AnimateBump(float dt)
        {
            if (bump <= 0f) return;

            bump = Mathf.Lerp(bump, 0f, 1f - Mathf.Exp(-bumpRecoverSpeed * dt));
            if (bump < BumpEpsilon) bump = 0f; // settle exactly, so we stop touching the text

            amountText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, bumpScale, bump);
            amountText.color = Color.Lerp(baseColor, flashColor, bump);
        }

        private void RollTowards(double target, float dt)
        {
            if (shown == target) return;

            shown += (target - shown) * (1.0 - Math.Exp(-rollSpeed * dt));
            if (Math.Abs(target - shown) < Math.Max(0.5, target * 0.0001)) shown = target;

            RefreshAmountText();
        }

        // Only rebuild the string when the visible number actually changes.
        private void RefreshAmountText()
        {
            double floor = Math.Floor(shown);
            if (floor == shownFloor) return;

            shownFloor = floor;
            amountText.text = NumberFormatter.Format(shown);
        }
    }
}
