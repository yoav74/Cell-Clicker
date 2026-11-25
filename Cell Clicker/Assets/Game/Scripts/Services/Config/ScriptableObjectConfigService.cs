using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellClicker.Platform
{
    /// <summary>
    /// Answers config requests with the assets Bootstrap hands it. Generic over the asset type, so this
    /// assembly never needs to know the game's config classes (Services references no Game code).
    /// </summary>
    public class ScriptableObjectConfigService : IConfigService
    {
        private readonly Dictionary<Type, ScriptableObject> configs = new Dictionary<Type, ScriptableObject>();

        public ScriptableObjectConfigService(params ScriptableObject[] assets)
        {
            foreach (ScriptableObject asset in assets)
            {
                if (asset == null)
                {
                    Debug.LogError("Cell Clicker: a config asset is missing. Select Services in the Bootstrap scene and fill in every config field.");
                    continue;
                }
                configs[asset.GetType()] = asset;
            }
        }

        public TConfig Get<TConfig>() where TConfig : ScriptableObject
        {
            if (configs.TryGetValue(typeof(TConfig), out ScriptableObject asset)) return (TConfig)asset;

            Debug.LogError($"Cell Clicker: no {typeof(TConfig).Name} registered. Start from the Bootstrap scene and assign it on Services.");
            return null;
        }
    }
}
