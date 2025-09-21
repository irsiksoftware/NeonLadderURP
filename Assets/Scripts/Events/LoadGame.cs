using NeonLadder.Debugging;
using NeonLadderURP.DataManagement;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired to load the game data.
    /// </summary>
    public class LoadGame : BaseGameEvent<LoadGame>
    {
        public override void Execute()
        {
            LoadPlayerData();
        }

        private void LoadPlayerData()
        {
            var playerData = SaveSystem.Load();
            if (playerData != null)
            {
                model.Player.PermaCurrency.current = playerData.PermaCurrency;
                model.Player.Unlocks.Set(playerData.Unlocks);
                ApplyPlayerSettings(playerData.Settings);
                Debugger.LogInformation(LogCategory.Player, "Player data loaded.");
            }
            else
            {
                Debugger.LogWarning(LogCategory.Player, "No save data found.");
            }
        }

        private void ApplyPlayerSettings(PlayerSettings settings)
        {
            PlayerPrefs.SetFloat("MusicVolume", settings.MusicVolume);
            PlayerPrefs.SetFloat("SFXVolume", settings.SFXVolume);
            PlayerPrefs.SetString("Resolution", settings.Resolution);
        }
    }
}
