using CellClicker.Platform;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CellClicker.Core
{
    /// <summary>
    /// The only script in the Bootstrap scene (first in the Scene List). Applies app settings, creates
    /// every service in order, then opens Home. Nothing else in the game creates services.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Tooltip("Android defaults to 30 FPS. 60 makes tapping feel much smoother.")]
        [SerializeField] private int targetFrameRate = 60;
        [Tooltip("The first screen, opened once every service is ready.")]
        [SerializeField] private string firstScene = "Home";

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            // Order matters: Sound and Haptics read the Settings, so Settings comes first.
            Services.Settings = new PlayerPrefsSettings();
            Services.Save = new JsonFileSave();
            Services.Sound = new AudioSourceSound(Services.Settings);
            Services.Haptics = new AndroidHaptics(Services.Settings);

            SceneManager.LoadScene(firstScene);
        }
    }
}
