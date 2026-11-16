using System;
using CellClicker.Platform;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Restores every <see cref="ISaveable"/> in the scene from <see cref="Services.Save"/> at startup and
    /// saves them back: every few seconds while playing, and when the app is paused, closed or the scene ends.
    /// </summary>
    // Runs its Start before the UI's Start, so the HUD shows the loaded values instead of rolling up from 0.
    // By then every Awake has run, so the systems are ready to be restored.
    [DefaultExecutionOrder(-100)]
    public class SaveSystem : MonoBehaviour
    {
        [Tooltip("Components that implement ISaveable, e.g. the DnaBank and OrganismRoster.")]
        [SerializeField] private MonoBehaviour[] saveables;
        [Tooltip("Seconds between autosaves while playing. The game also saves when it is paused or closed.")]
        [SerializeField] private float autosaveInterval = 5f;

        private ISaveable[] targets = Array.Empty<ISaveable>();
        private ISaveService save;
        private bool loaded;
        private float nextAutosaveTime;

        /// <summary>When the save was last written, or null for a new player. For Offline Earnings.</summary>
        public DateTime? LastSavedUtc => save?.LastSavedUtc;

        private void Awake()
        {
            save = Services.Save;
            if (save == null)
            {
                Debug.LogError("Cell Clicker: no save service. Start the game from the Bootstrap scene.", this);
                enabled = false;
                return;
            }
            CollectTargets();
        }

        private void Start()
        {
            Restore();
            loaded = true;
            ScheduleAutosave();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextAutosaveTime) Save();
        }

        // Android may kill a backgrounded app without calling OnApplicationQuit, so pause is the important one.
        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit() => Save();

        // Leaving the Level for the Home screen destroys this object; save so no progress is lost.
        private void OnDestroy() => Save();

        public void Save()
        {
            // Never write before the old data was restored, or a fresh state would overwrite real progress.
            if (!loaded) return;

            ScheduleAutosave();
            foreach (ISaveable target in targets) save.Store(target.SaveKey, target.CaptureState());
            save.Flush(); // on failure the service logs it; the next autosave tries again
        }

        private void Restore()
        {
            foreach (ISaveable target in targets)
            {
                string json = save.Load(target.SaveKey);
                if (json == null) continue;

                // One broken section shouldn't stop the rest of the progress from loading.
                try
                {
                    target.RestoreState(json);
                }
                catch (ArgumentException e)
                {
                    Debug.LogWarning($"Cell Clicker: could not load '{target.SaveKey}' from the save: {e.Message}");
                }
            }
        }

        private void ScheduleAutosave() => nextAutosaveTime = Time.unscaledTime + Mathf.Max(1f, autosaveInterval);

        private void CollectTargets()
        {
            if (saveables == null) return;

            int count = 0;
            targets = new ISaveable[saveables.Length];
            foreach (MonoBehaviour component in saveables)
            {
                if (component is ISaveable saveable) targets[count++] = saveable;
                else if (component != null) Debug.LogError($"{component.name} does not implement ISaveable.", this);
            }
            Array.Resize(ref targets, count);
        }

        private void OnValidate()
        {
            if (saveables == null) return;
            foreach (MonoBehaviour component in saveables)
            {
                if (component != null && component is not ISaveable)
                    Debug.LogWarning($"{component.name} does not implement ISaveable and will not be saved.", this);
            }
        }
    }
}
