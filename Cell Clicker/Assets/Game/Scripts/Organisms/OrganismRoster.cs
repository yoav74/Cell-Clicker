using System;
using System.Collections.Generic;
using CellClicker.Core;
using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Organisms
{
    /// <summary>
    /// Owns the player's Organisms and levels them up with DNA. Reports their combined DNA/sec as an
    /// <see cref="IDnaSource"/>; the <see cref="DnaGenerator"/> turns that into DNA.
    /// Which Organisms exist, and in what order, comes from <see cref="EconomyConfig"/> through <c>Services.Config</c>;
    /// Upgrades (through <see cref="IDnaModifiers"/>) can make every Organism Level cheaper.
    /// </summary>
    public class OrganismRoster : MonoBehaviour, IDnaSource, ISaveable
    {
        [SerializeField] private DnaBank bank;
        [Tooltip("Optional. A component that implements IDnaModifiers, e.g. the UpgradeSystem.")]
        [SerializeField] private MonoBehaviour modifiersSource;

        private IDnaModifiers modifiers;

        private readonly List<Organism> organisms = new List<Organism>();

        public IReadOnlyList<Organism> Organisms => organisms;
        public double DnaPerSecond { get; private set; }

        public event Action RateChanged;

        /// <summary>Fired after an Organism gains an Organism Level.</summary>
        public event Action<Organism> LevelChanged;

        /// <summary>Fired when an Upgrade changes every Organism's price.</summary>
        public event Action CostsChanged;

        public string SaveKey => "organisms";

        private void Awake()
        {
            modifiers = modifiersSource as IDnaModifiers;
            if (modifiersSource != null && modifiers == null)
                Debug.LogError($"{modifiersSource.name} does not implement IDnaModifiers.", this);

            OrganismDefinition[] definitions = Services.Config.Economy()?.Organisms;
            if (definitions == null) return;

            foreach (OrganismDefinition definition in definitions)
            {
                if (definition != null) organisms.Add(new Organism(definition));
            }
        }

        /// <summary>The first Organism is always available; each next one after the previous is recruited.</summary>
        public bool IsUnlocked(Organism organism)
        {
            int index = organisms.IndexOf(organism);
            return index == 0 || (index > 0 && organisms[index - 1].IsRecruited);
        }

        private void OnEnable()
        {
            if (modifiers != null) modifiers.Changed += OnModifiersChanged;
        }

        private void OnDisable()
        {
            if (modifiers != null) modifiers.Changed -= OnModifiersChanged;
        }

        /// <summary>The DNA the next Organism Level costs, after Upgrade discounts. Never below 1.</summary>
        public double CostOf(Organism organism)
        {
            double cost = organism.NextCost;
            if (modifiers != null) cost = Math.Max(1, Math.Floor(cost * modifiers.OrganismCostMultiplier));
            return cost;
        }

        public bool CanLevelUp(Organism organism) => IsUnlocked(organism) && bank.CanAfford(CostOf(organism));

        public bool TryLevelUp(Organism organism)
        {
            if (!IsUnlocked(organism) || !bank.TrySpend(CostOf(organism))) return false;

            organism.LevelUp();
            RecalculateRate();
            LevelChanged?.Invoke(organism);
            return true;
        }

        public string CaptureState()
        {
            var state = new State();
            foreach (Organism organism in organisms)
            {
                state.organisms.Add(new SavedOrganism { id = organism.Definition.SaveId, level = organism.Level });
            }
            return JsonUtility.ToJson(state);
        }

        public void RestoreState(string json)
        {
            State state = JsonUtility.FromJson<State>(json);
            if (state?.organisms == null) return;

            // Matched by id, not by position, so designers can reorder or add Organisms without breaking saves.
            foreach (SavedOrganism saved in state.organisms)
            {
                Organism organism = organisms.Find(o => o.Definition.SaveId == saved.id);
                if (organism == null || saved.level <= 0) continue;

                organism.SetLevel(saved.level);
                LevelChanged?.Invoke(organism);
            }
            RecalculateRate();
        }

        private void OnModifiersChanged() => CostsChanged?.Invoke();

        private void RecalculateRate()
        {
            double total = 0;
            foreach (Organism organism in organisms) total += organism.DnaPerSecond;

            DnaPerSecond = total;
            RateChanged?.Invoke();
        }

        [Serializable]
        private class State
        {
            public List<SavedOrganism> organisms = new List<SavedOrganism>();
        }

        [Serializable]
        private class SavedOrganism
        {
            public string id;
            public int level;
        }
    }
}
