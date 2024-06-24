using NeonLadderURP.Models;
using System;
using System.Collections.Generic;

namespace NeonLadderURP.DataManagement
{
    [Serializable]
    public class PlayerData
    {
        public int PermaCurrency;
        public List<Unlock> Unlocks;
        public PlayerSettings Settings; // Custom PlayerSettings class
        public int SaveVersion; // For versioning

        public void Reset()
        {
            PermaCurrency = 0;
            foreach (var unlock in Unlocks)
            {
                unlock.IsPurchased = false;
            }
            Settings = new PlayerSettings();
        }

        public bool PurchaseUnlock(string unlockName)
        {
            var unlock = Unlocks.Find(u => u.Name == unlockName);
            if (unlock != null && !unlock.IsPurchased && PermaCurrency >= unlock.Cost)
            {
                PermaCurrency -= unlock.Cost;
                unlock.IsPurchased = true;
                return true;
            }
            return false;
        }

        public bool RefundUnlock(string unlockName)
        {
            var unlock = Unlocks.Find(u => u.Name == unlockName);
            if (unlock != null && unlock.IsPurchased)
            {
                PermaCurrency += unlock.Cost;
                unlock.IsPurchased = false;
                return true;
            }
            return false;
        }
    }
}
