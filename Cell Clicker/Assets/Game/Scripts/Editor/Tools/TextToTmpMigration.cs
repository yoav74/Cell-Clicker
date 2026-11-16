using CellClicker.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// Menu: Cell Clicker > Convert Text To TMP.
    /// Swaps every legacy uGUI Text under "Hive Canvas" for TextMeshProUGUI with the same text, size,
    /// color and style, then re-wires the scripts that pointed at them. A GameObject can only have one
    /// text component, so the old one has to be removed. This is undoable (Ctrl+Z).
    /// </summary>
    public static class TextToTmpMigration
    {
        [MenuItem("Cell Clicker/Convert Text To TMP")]
        public static void ConvertFromMenu() => TmpSetup.RunWhenReady(Convert);

        private static void Convert()
        {
            GameObject canvasGo = GameObject.Find("Hive Canvas");
            if (canvasGo == null)
            {
                EditorUtility.DisplayDialog("Cell Clicker", "No 'Hive Canvas' in the open scene.", "OK");
                return;
            }

            TMP_FontAsset font = TmpSetup.DefaultFont;
            int converted = 0;

            foreach (var legacy in canvasGo.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                Convert(legacy, font);
                converted++;
            }

            Rewire(canvasGo, font);
            EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            Debug.Log($"Cell Clicker: converted {converted} text(s) to TextMeshPro. Save the scene (Ctrl+S).");
        }

        private static void Convert(UnityEngine.UI.Text legacy, TMP_FontAsset font)
        {
            GameObject go = legacy.gameObject;

            // Copy what we need before removing it.
            string content = legacy.text;
            float size = legacy.fontSize;
            Color color = legacy.color;
            bool bold = legacy.fontStyle == FontStyle.Bold || legacy.fontStyle == FontStyle.BoldAndItalic;
            bool raycast = legacy.raycastTarget;

            // uGUI Outline/Shadow don't work with TMP; TMP materials do that job instead.
            foreach (var effect in go.GetComponents<UnityEngine.UI.BaseMeshEffect>())
            {
                Undo.DestroyObjectImmediate(effect);
            }
            Undo.DestroyObjectImmediate(legacy);

            var tmp = Undo.AddComponent<TextMeshProUGUI>(go);
            tmp.font = font;
            tmp.text = content;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = raycast;
            tmp.fontSharedMaterial = go.name == "DNA Amount" ? TmpSetup.GlowPreset : TmpSetup.SoftShadowPreset;
        }

        // Point the scripts at the new components (their old references died with the legacy Text).
        private static void Rewire(GameObject canvasGo, TMP_FontAsset font)
        {
            Transform header = canvasGo.transform.Find("Safe Area/Header");
            var counter = canvasGo.GetComponent<DnaCounterUI>();
            if (counter != null && header != null)
            {
                Wire(counter, "amountText", header.Find("DNA Amount")?.GetComponent<TMP_Text>());
                Wire(counter, "perTapText", header.Find("Per Tap")?.GetComponent<TMP_Text>());
            }

            var spawner = canvasGo.GetComponentInChildren<FloatingTextSpawner>(true);
            if (spawner != null)
            {
                Wire(spawner, "font", font);
                Wire(spawner, "material", TmpSetup.PopupPreset);
            }
        }

        private static void Wire(Object target, string field, Object value)
        {
            if (value == null)
            {
                Debug.LogWarning($"Cell Clicker: nothing to wire into {target.name}.{field}.");
                return;
            }

            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
