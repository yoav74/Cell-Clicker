using CellClicker.Platform;

namespace CellClicker.Core
{
    /// <summary>Shortcut so game code reads <c>Services.Config.Economy()</c> instead of the generic lookup.</summary>
    public static class ConfigServiceExtensions
    {
        public static EconomyConfig Economy(this IConfigService config) => config?.Get<EconomyConfig>();
    }
}
