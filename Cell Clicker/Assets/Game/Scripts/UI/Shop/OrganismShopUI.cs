using CellClicker.Core;
using CellClicker.Organisms;
using UnityEngine;

namespace CellClicker.UI
{
    /// <summary>Fills the Organisms popup with one row per Organism, once, from a template row.</summary>
    public class OrganismShopUI : MonoBehaviour
    {
        [SerializeField] private OrganismRoster roster;
        [SerializeField] private DnaBank bank;
        [SerializeField] private OrganismRowUI rowTemplate;
        [SerializeField] private RectTransform content;

        private void Awake()
        {
            rowTemplate.gameObject.SetActive(false);

            foreach (Organism organism in roster.Organisms)
            {
                OrganismRowUI row = Instantiate(rowTemplate, content);
                row.name = organism.Definition.DisplayName;
                row.Bind(organism, roster, bank);
                row.gameObject.SetActive(true);
            }
        }
    }
}
