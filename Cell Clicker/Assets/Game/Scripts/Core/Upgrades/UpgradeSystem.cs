using System;
using System.Collections.Generic;
using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Owns the player's Upgrades (the list comes from <see cref="EconomyConfig"/>), buys their levels with DNA
    /// and saves them. Combines every Upgrade's effect into the <see cref="IDnaModifiers"/> that the
    /// generator, Tap Power and the Organism roster read.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour, IDnaModifiers, ISaveable
    {
        [SerializeField] private DnaBank bank;

        private readonly List<Upgrade> upgrades = new List<Upgrade>();
        private readonly DnaModifiers modifiers = new DnaModifiers();

        public IReadOnlyList<Upgrade> Upgrades => upgrades;

        public double AllDnaMultiplier => modifiers.AllDna;
        public double TapPowerMultiplier => modifiers.TapPower;
        public double OrganismCostMultiplier => modifiers.OrganismCost;

        /// <summary>Fired after an Upgrade level is bought or loaded (the multipliers are already updated).</summary>
        public event Action Changed;

        public string SaveKey => "upgrades";

        private void Awake()
        {
            UpgradeDefinition[] definitions = Services.Config.Economy()?.Upgrades;
            if (definitions == null) return;

            foreach (UpgradeDefinition definition in definitions)
            {
                if (definition != null) upgrades.Add(new Upgrade(definition));
            }
        }

        public bool CanBuy(Upgrade upgrade) => !upgrade.IsMaxed && bank.CanAfford(upgrade.NextCost);

        public bool TryBuy(Upgrade upgrade)
        {
            if (upgrade.IsMaxed || !bank.TrySpend(upgrade.NextCost)) return false;

            upgrade.LevelUp();
            Recalculate();
            return true;
        }

        public string CaptureState()
        {
            var state = new State();
            foreach (Upgrade upgrade in upgrades)
            {
                state.upgrades.Add(new SavedUpgrade { id = upgrade.Definition.SaveId, level = upgrade.Level });
            }
            return JsonUtility.ToJson(state);
        }

        public void RestoreState(string json)
        {
            State state = JsonUtility.FromJson<State>(json);
            if (state?.upgrades == null) return;

            // Matched by id, not by position, so designers can reorder or add Upgrades without breaking saves.
            foreach (SavedUpgrade saved in state.upgrades)
            {
                Upgrade upgrade = upgrades.Find(u => u.Definition.SaveId == saved.id);
                upgrade?.SetLevel(saved.level);
            }
            Recalculate();
        }

        private void Recalculate()
        {
            modifiers.Reset();
            foreach (Upgrade upgrade in upgrades)
            {
                if (upgrade.Level > 0) upgrade.Definition.ApplyTo(modifiers, upgrade.Level);
            }
            Changed?.Invoke();
        }

        [Serializable]
        private class State
        {
            public List<SavedUpgrade> upgrades = new List<SavedUpgrade>();
        }

        [Serializable]
        private class SavedUpgrade
        {
            public string id;
            public int level;
        }
    }
}
