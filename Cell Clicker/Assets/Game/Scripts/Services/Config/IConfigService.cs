using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// Where the game reads its tuning (prices, rewards, rates). Today the values come from config assets;
    /// later a Remote Config service can answer the same calls with server values on top.
    /// </summary>
    public interface IConfigService
    {
        /// <summary>The config asset of type <typeparamref name="TConfig"/>, or null if none was registered.</summary>
        TConfig Get<TConfig>() where TConfig : ScriptableObject;
    }
}
