using NeonLadder.Core;
using NeonLadderURP.DataManagement;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired to save the game data.
    /// </summary>
    public class SaveGame : BaseGameEvent<SaveGame>
    {
        public override void Execute()
        {
            SavePlayerData();
        }

        private void SavePlayerData()
        {
            PlayerData playerData = new PlayerData
            {
                PermaCurrency = model.Player.PermaCurrency.current,
                Unlocks = model.Player.Unlocks.Get(), // Assume you have a method to get current unlocks
                Settings = new PlayerSettings()
                {
                    MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f),
                    SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f),
                    Resolution = PlayerPrefs.GetString("Resolution", "1920x1080")
                },
                SaveVersion = 1
            };

            SaveSystem.Save(playerData);
            Debug.Log("Player data saved.");
        }
    }
}
