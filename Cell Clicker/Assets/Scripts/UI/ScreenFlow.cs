using UnityEngine;
using UnityEngine.SceneManagement;

namespace CellClicker.UI
{
    /// <summary>Buttons that change screens. Wire a button's On Click () to Play or Back.</summary>
    public class ScreenFlow : MonoBehaviour
    {
        [Tooltip("Exact scene names; both must be in File > Build Profiles > Scene List.")]
        [SerializeField] private string levelScene = "Level";
        [SerializeField] private string homeScene = "Home";

        // The Level's SaveSystem saves in OnDestroy, so leaving the Level never loses progress.
        public void Play() => SceneManager.LoadScene(levelScene);

        public void Back() => SceneManager.LoadScene(homeScene);
    }
}
