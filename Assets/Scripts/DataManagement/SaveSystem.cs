using NeonLadderURP.Models;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeonLadderURP.DataManagement
{
    public static class SaveSystem
    {
        private const string SaveFileName = "playerData.json";

        public static void Save(PlayerData data)
        {
            string json = JsonUtility.ToJson(data);
            // Optionally encrypt json here
            File.WriteAllText(Path.Combine(Application.persistentDataPath, SaveFileName), json);
        }

        public static PlayerData Load()
        {
            string path = Path.Combine(Application.persistentDataPath, SaveFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                // Optionally decrypt json here
                return JsonUtility.FromJson<PlayerData>(json);
            }
            return new PlayerData { Unlocks = new List<Unlock>(), Settings = new PlayerSettings() };
        }
    }
}
