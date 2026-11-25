using System;
using CellClicker.Hive;
using CellClicker.Organisms;
using UnityEngine;

namespace CellClicker.Core
{
    /// <summary>
    /// The economy in one place, read through <c>Services.Config</c>: points at the Tap Power, Organism,
    /// Upgrade and Offline Earnings assets, so a designer finds all the tuning from here and a Remote Config
    /// service can later swap any of them. Values match the economy sheet (linked in the README).
    /// </summary>
    [CreateAssetMenu(fileName = "Economy Config", menuName = "Cell Clicker/Economy Config")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Sources and per-session sinks")]
        [SerializeField] private TapPowerDefinition tapPower;
        [Tooltip("In unlock order: each Organism is revealed once the one before it is recruited.")]
        [SerializeField] private OrganismDefinition[] organisms = Array.Empty<OrganismDefinition>();
        [SerializeField] private OfflineEarningsSettings offlineEarnings;

        [Header("Upgrades, the big sink")]
        [Tooltip("In shop order. Each is an asset (Create > Cell Clicker > Upgrades).")]
        [SerializeField] private UpgradeDefinition[] upgrades = Array.Empty<UpgradeDefinition>();

        public TapPowerDefinition TapPower => tapPower;
        public OrganismDefinition[] Organisms => organisms;
        public OfflineEarningsSettings OfflineEarnings => offlineEarnings;
        public UpgradeDefinition[] Upgrades => upgrades;
    }
}
