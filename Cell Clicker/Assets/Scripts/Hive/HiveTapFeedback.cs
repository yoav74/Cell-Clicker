using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>
    /// Base class for anything that reacts to a hive tap (squish, popups, particles, sound...).
    /// To add a new effect, make a new subclass. No need to touch <see cref="DnaHive"/>.
    /// </summary>
    public abstract class HiveTapFeedback : MonoBehaviour
    {
        [SerializeField] private DnaHive hive;

        protected DnaHive Hive => hive;

        protected virtual void OnEnable()
        {
            if (hive != null) hive.Tapped += OnHiveTapped;
        }

        protected virtual void OnDisable()
        {
            if (hive != null) hive.Tapped -= OnHiveTapped;
        }

        protected abstract void OnHiveTapped(HiveTap tap);
    }
}
