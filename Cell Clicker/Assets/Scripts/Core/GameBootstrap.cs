using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>Runtime settings that should be applied once when the game starts.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Android defaults to 30 FPS. 60 makes tapping feel much smoother.")]
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
