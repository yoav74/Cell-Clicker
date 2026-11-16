using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Holds the player's DNA. Everything that earns DNA goes through here,
    /// and the UI listens to <see cref="Changed"/> to update itself.
    /// </summary>
    public class DnaBank : MonoBehaviour, ISaveable
    {
        [SerializeField] private double startingDna = 0;

        public double Amount { get; private set; }

        /// <summary>Fired with the new total whenever DNA changes.</summary>
        public event Action<double> Changed;

        public string SaveKey => "dna";

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

        public string CaptureState() => JsonUtility.ToJson(new State { amount = Amount });

        public void RestoreState(string json)
        {
            double saved = JsonUtility.FromJson<State>(json).amount;
            if (!double.IsFinite(saved) || saved < 0) return; // a broken value must never wipe or poison the bank

            Amount = saved;
            Changed?.Invoke(Amount);
        }

        [Serializable]
        private class State
        {
            public double amount;
        }
    }
}
