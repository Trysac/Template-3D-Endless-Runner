using GauntletRun.Core;
using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>Keeps the profile in PlayerPrefs (the registry on Windows, IndexedDB on the web).</summary>
    public sealed class PlayerPrefsProfileStore : IProfileStore
    {
        public const string Key = "GauntletRun.Profile";

        public Profile Load() => Profile.Parse(PlayerPrefs.GetString(Key, string.Empty));

        public void Save(Profile profile)
        {
            PlayerPrefs.SetString(Key, profile.Serialize());
            PlayerPrefs.Save();
        }
    }
}
