using System;

namespace CellClicker.Core
{
    /// <summary>
    /// The boosts bought with Upgrades, read by the systems they change: DnaGenerator and TapPower
    /// (all DNA), TapPower (Tap Power) and OrganismRoster (Organism cost). All are 1 with no Upgrades.
    /// </summary>
    public interface IDnaModifiers
    {
        /// <summary>Multiplies every DNA earned: taps and DNA/sec. 1.5 = +50%.</summary>
        double AllDnaMultiplier { get; }

        /// <summary>Multiplies the DNA per tap, on top of <see cref="AllDnaMultiplier"/>.</summary>
        double TapPowerMultiplier { get; }

        /// <summary>Multiplies every Organism Level's price. 0.9 = 10% cheaper.</summary>
        double OrganismCostMultiplier { get; }

        /// <summary>Fired whenever any of the multipliers changes.</summary>
        event Action Changed;
    }
}
