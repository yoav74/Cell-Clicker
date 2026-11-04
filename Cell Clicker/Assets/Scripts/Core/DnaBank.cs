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
    }
}
