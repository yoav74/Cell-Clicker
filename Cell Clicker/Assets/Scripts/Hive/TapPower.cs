using System;
using CellClicker.Core;
using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>
    /// The player's Tap Power: its level, bought with DNA, and the DNA each tap on the Hive earns.
    /// <see cref="DnaHive"/> reads <see cref="Value"/>; the UI listens to <see cref="Changed"/>.
    /// </summary>
    public class TapPower : MonoBehaviour, ISaveable
    {
        [SerializeField] private DnaBank bank;
        [SerializeField] private TapPowerDefinition definition;

        public TapPowerDefinition Definition => definition;

        /// <summary>0 = never upgraded.</summary>
        public int Level { get; private set; }

        public double Value => definition.TapPowerAt(Level);
        public double NextCost => definition.CostAt(Level);
        public double NextLevelGain => definition.TapPowerAt(Level + 1) - Value;
        public bool CanLevelUp => bank.CanAfford(NextCost);

        /// <summary>Fired after the level changes (bought or loaded).</summary>
        public event Action Changed;

        public string SaveKey => "tapPower";

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

        [Serializable]
        private class State
        {
            public int level;
        }
    }
}
