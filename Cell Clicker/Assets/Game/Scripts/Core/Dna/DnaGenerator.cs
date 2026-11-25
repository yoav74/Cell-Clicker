using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Adds the DNA/sec of every <see cref="IDnaSource"/>, times the <see cref="IDnaModifiers"/> all-DNA boost, to the
    /// <see cref="DnaBank"/> each frame.
    /// The UI reads the total DNA/sec from here, so it never needs to know where DNA comes from.
    /// </summary>
    public class DnaGenerator : MonoBehaviour
    {
        [SerializeField] private DnaBank bank;
        [Tooltip("Components that implement IDnaSource, e.g. the OrganismRoster.")]
        [SerializeField] private MonoBehaviour[] sources;
        [Tooltip("Optional. A component that implements IDnaModifiers, e.g. the UpgradeSystem.")]
        [SerializeField] private MonoBehaviour modifiersSource;

        private IDnaSource[] dnaSources = Array.Empty<IDnaSource>();
        private IDnaModifiers modifiers;

        public double DnaPerSecond { get; private set; }

        /// <summary>Fired with the new total DNA/sec.</summary>
        public event Action<double> RateChanged;

        private void Awake()
        {
            modifiers = modifiersSource as IDnaModifiers;
            if (modifiersSource != null && modifiers == null)
                Debug.LogError($"{modifiersSource.name} does not implement IDnaModifiers.", this);

            if (sources == null) return;

            int count = 0;
            dnaSources = new IDnaSource[sources.Length];
            foreach (MonoBehaviour source in sources)
            {
                if (source is IDnaSource dnaSource) dnaSources[count++] = dnaSource;
                else if (source != null) Debug.LogError($"{source.name} does not implement IDnaSource.", this);
            }
            Array.Resize(ref dnaSources, count);
        }

        private void OnEnable()
        {
            foreach (IDnaSource source in dnaSources) source.RateChanged += Recalculate;
            if (modifiers != null) modifiers.Changed += Recalculate;
            Recalculate();
        }

        private void OnDisable()
        {
            foreach (IDnaSource source in dnaSources) source.RateChanged -= Recalculate;
            if (modifiers != null) modifiers.Changed -= Recalculate;
        }

        private void Update()
        {
            if (DnaPerSecond > 0) bank.Add(DnaPerSecond * Time.deltaTime);
        }

        private void Recalculate()
        {
            double total = 0;
            foreach (IDnaSource source in dnaSources) total += source.DnaPerSecond;
            if (modifiers != null) total *= modifiers.AllDnaMultiplier;

            DnaPerSecond = total;
            RateChanged?.Invoke(total);
        }

        private void OnValidate()
        {
            if (sources == null) return;
            foreach (MonoBehaviour source in sources)
            {
                if (source != null && source is not IDnaSource)
                    Debug.LogWarning($"{source.name} does not implement IDnaSource and will be ignored.", this);
            }
            if (modifiersSource != null && modifiersSource is not IDnaModifiers)
                Debug.LogWarning($"{modifiersSource.name} does not implement IDnaModifiers and will be ignored.", this);
        }
    }
}
