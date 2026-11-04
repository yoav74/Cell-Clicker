using UnityEngine;
using UnityEngine.UI;

namespace CellClicker.UI
{
    /// <summary>
    /// Shows a code-drawn placeholder sprite on this Image, in Play mode and in the editor.
    /// Uses overrideSprite, which is never saved, so the scene doesn't change. Remove this
    /// component and set a normal sprite once real art exists.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class ProceduralImage : MonoBehaviour
    {
        [SerializeField] private ProceduralSpriteKind kind;

        private void OnEnable() => Apply();

        private void OnValidate()
        {
            if (isActiveAndEnabled) Apply();
        }

        private void Apply()
        {
            GetComponent<Image>().overrideSprite = ProceduralSprites.Get(kind);
        }
    }
}
