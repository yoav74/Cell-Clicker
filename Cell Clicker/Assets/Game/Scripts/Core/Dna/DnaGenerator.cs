using System;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Adds the DNA/sec of every <see cref="IDnaSource"/> to the <see cref="DnaBank"/> each frame.
    /// The UI reads the total DNA/sec from here, so it never needs to know where DNA comes from.
    /// </summary>
    public class DnaGenerator : MonoBehaviour
    {
        [SerializeField] private DnaBank bank;
        [Tooltip("Components that implement IDnaSource, e.g. the OrganismRoster.")]
        [SerializeField] private MonoBehaviour[] sources;

        private IDnaSource[] dnaSources = Array.Empty<IDnaSource>();

        public double DnaPerSecond { get; private set; }

        /// <summary>Fired with the new total DNA/sec.</summary>
        public event Action<double> RateChanged;

        private void Awake()
        {
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
            Recalculate();
        }

        private void OnDisable()
        {
            foreach (IDnaSource source in dnaSources) source.RateChanged -= Recalculate;
        }

        private void Update()
        {
            if (DnaPerSecond > 0) bank.Add(DnaPerSecond * Time.deltaTime);
        }

        private void Recalculate()
        {
            double total = 0;
            foreach (IDnaSource source in dnaSources) total += source.DnaPerSecond;

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
        }
    }
}
