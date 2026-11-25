namespace CellClicker.Core
{
    /// <summary>
    /// The combined multipliers of every Upgrade. <see cref="UpgradeSystem"/> resets it and lets each
    /// <see cref="UpgradeDefinition"/> multiply in its own effect, so a new kind of Upgrade never edits this list's users.
    /// </summary>
    public class DnaModifiers
    {
        public double AllDna { get; private set; } = 1.0;
        public double TapPower { get; private set; } = 1.0;
        public double OrganismCost { get; private set; } = 1.0;

        public void Reset()
        {
            AllDna = 1.0;
            TapPower = 1.0;
            OrganismCost = 1.0;
        }

        public void MultiplyAllDna(double factor) => AllDna *= factor;
        public void MultiplyTapPower(double factor) => TapPower *= factor;
        public void MultiplyOrganismCost(double factor) => OrganismCost *= factor;
    }
}
