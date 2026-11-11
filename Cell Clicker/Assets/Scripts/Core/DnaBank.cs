using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Holds the player's DNA. Everything that earns DNA goes through here,
    /// and the UI listens to <see cref="Changed"/> to update itself.
    /// </summary>
    public class DnaBank : MonoBehaviour
    {
        [SerializeField] private double startingDna = 0;

        public double Amount { get; private set; }

        /// <summary>Fired with the new total whenever DNA changes.</summary>
        public event Action<double> Changed;

        private void Awake()
        {
            Amount = startingDna;
        }

        public void Add(double amount)
        {
            if (amount <= 0) return;

            Amount += amount;
            Changed?.Invoke(Amount);
        }

        public bool CanAfford(double cost) => cost >= 0 && Amount >= cost;

        /// <summary>Removes <paramref name="cost"/> if the player has enough. Returns false and changes nothing otherwise.</summary>
        public bool TrySpend(double cost)
        {
            if (!CanAfford(cost)) return false;

            Amount -= cost;
            Changed?.Invoke(Amount);
            return true;
        }
    }
}
