using System;
using System.IO;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// Loads every <see cref="ISaveable"/> at startup and saves them to a local JSON file:
    /// every few seconds while playing, and when the app is paused (sent to background) or closed.
    /// </summary>
    // Runs its Start before the UI's Start, so the HUD shows the loaded values instead of rolling up from 0.
    // By then every Awake has run, so the systems are ready to be restored.
    [DefaultExecutionOrder(-100)]
    public class SaveSystem : MonoBehaviour
    {
        public const string FileName = "save.json";

        [Tooltip("Components that implement ISaveable, e.g. the DnaBank and OrganismRoster.")]
        [SerializeField] private MonoBehaviour[] saveables;
        [Tooltip("Seconds between autosaves while playing. The game also saves when it is paused or closed.")]
        [SerializeField] private float autosaveInterval = 5f;

        private ISaveable[] targets = Array.Empty<ISaveable>();
        private SaveFileStore store;
        private SaveFile file = new SaveFile();
        private bool loaded;
        private float nextAutosaveTime;

        public static string DefaultPath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>When the save was last written (loaded, or by this session), or null for a new player. For Offline Earnings.</summary>
        public DateTime? LastSavedUtc { get; private set; }

        private void Awake()
        {
            store = new SaveFileStore(DefaultPath);
            CollectTargets();
        }

        private void Start()
        {
            Load();
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
            // Never write before the old file was read, or a fresh state would overwrite real progress.
            if (!loaded) return;

            ScheduleAutosave();
            foreach (ISaveable target in targets) file.Set(target.SaveKey, target.CaptureState());
            file.version = SaveFile.CurrentVersion;
            file.savedAtUtcTicks = DateTime.UtcNow.Ticks;

            try
            {
                store.Write(file);
                LastSavedUtc = DateTime.UtcNow;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // Keep playing; the next autosave tries again.
                Debug.LogWarning($"Cell Clicker: could not save: {e.Message}");
            }
        }

        private void Load()
        {
            if (!store.TryLoad(out SaveFile saved)) return;

            // Keep the loaded file (not a new one) so data from systems not in this scene is never dropped.
            file = saved;
            if (TryGetUtc(saved.savedAtUtcTicks, out DateTime savedAt)) LastSavedUtc = savedAt;

            foreach (ISaveable target in targets)
            {
                string json = file.Find(target.SaveKey);
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

        /// <summary>Turns saved ticks into a UTC time; false if they are missing or out of range (broken file).</summary>
        public static bool TryGetUtc(long ticks, out DateTime utc)
        {
            bool valid = ticks > DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks;
            utc = valid ? new DateTime(ticks, DateTimeKind.Utc) : default;
            return valid;
        }

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
