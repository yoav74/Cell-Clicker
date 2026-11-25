using System;
using CellClicker.Core;
using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>
    /// The player's Tap Power: its level, bought with DNA, and the DNA each tap on the Hive earns.
    /// The numbers come from <see cref="EconomyConfig"/> through <c>Services.Config</c>; Upgrades (through
    /// <see cref="IDnaModifiers"/>) multiply every tap.
    /// <see cref="DnaHive"/> reads <see cref="Value"/>; the UI listens to <see cref="Changed"/>.
    /// </summary>
    public class TapPower : MonoBehaviour, ISaveable
    {
        [SerializeField] private DnaBank bank;
        [Tooltip("Optional. A component that implements IDnaModifiers, e.g. the UpgradeSystem.")]
        [SerializeField] private MonoBehaviour modifiersSource;

        private TapPowerDefinition definition;
        private IDnaModifiers modifiers;

        // Looked up on first use, not in Awake: the shop row's Awake may ask before this one runs.
        public TapPowerDefinition Definition => definition != null ? definition : definition = Services.Config.Economy().TapPower;

        /// <summary>0 = never upgraded.</summary>
        public int Level { get; private set; }

        public double Value => Definition.TapPowerAt(Level) * Boost;
        public double NextCost => Definition.CostAt(Level);
        public double NextLevelGain => (Definition.TapPowerAt(Level + 1) - Definition.TapPowerAt(Level)) * Boost;
        public bool CanLevelUp => bank.CanAfford(NextCost);

        private double Boost => modifiers == null ? 1.0 : modifiers.AllDnaMultiplier * modifiers.TapPowerMultiplier;

        /// <summary>Fired after the level or an Upgrade boost changes (bought or loaded).</summary>
        public event Action Changed;

        public string SaveKey => "tapPower";

        private void Awake()
        {
            modifiers = modifiersSource as IDnaModifiers;
            if (modifiersSource != null && modifiers == null)
                Debug.LogError($"{modifiersSource.name} does not implement IDnaModifiers.", this);
        }

        private void OnEnable()
        {
            if (modifiers != null) modifiers.Changed += OnModifiersChanged;
        }

        private void OnDisable()
        {
            if (modifiers != null) modifiers.Changed -= OnModifiersChanged;
        }

        public bool TryLevelUp()
        {
            if (!bank.TrySpend(NextCost)) return false;

            Level++;
            Changed?.Invoke();
            return true;
        }

        public string CaptureState() => JsonUtility.ToJson(new State { level = Level });

        public void RestoreState(string json)
        {
            Level = Math.Max(0, JsonUtility.FromJson<State>(json).level);
            Changed?.Invoke();
        }

        private void OnModifiersChanged() => Changed?.Invoke();

        [Serializable]
        private class State
        {
            public int level;
        }
    }
}
