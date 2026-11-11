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
    public class OrganismRoster : MonoBehaviour, IDnaSource
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

        private void RecalculateRate()
        {
            double total = 0;
            foreach (Organism organism in organisms) total += organism.DnaPerSecond;

            DnaPerSecond = total;
            RateChanged?.Invoke();
        }
    }
}
