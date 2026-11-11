using System;
using System.Collections.Generic;
using CellClicker.Core;
using UnityEngine;

namespace CellClicker.Organisms
{
    /// <summary>
    /// Owns the player's Organisms and levels them up with DNA. Reports their combined DNA/sec as an
    /// <see cref="IDnaSource"/>; the <see cref="DnaGenerator"/> turns that into DNA.
    /// </summary>
    public class OrganismRoster : MonoBehaviour, IDnaSource, ISaveable
    {
        [SerializeField] private DnaBank bank;
        [Tooltip("In unlock order: each Organism is revealed once the one before it is recruited.")]
        [SerializeField] private OrganismDefinition[] definitions;

        private readonly List<Organism> organisms = new List<Organism>();

        public IReadOnlyList<Organism> Organisms => organisms;
        public double DnaPerSecond { get; private set; }

        public event Action RateChanged;

        /// <summary>Fired after an Organism gains an Organism Level.</summary>
        public event Action<Organism> LevelChanged;

        public string SaveKey => "organisms";

        private void Awake()
        {
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

        public bool CanLevelUp(Organism organism) => IsUnlocked(organism) && bank.CanAfford(organism.NextCost);

        public bool TryLevelUp(Organism organism)
        {
            if (!IsUnlocked(organism) || !bank.TrySpend(organism.NextCost)) return false;

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
