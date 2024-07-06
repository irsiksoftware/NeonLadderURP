using NeonLadderURP.Models;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnlock : MonoBehaviour
{
    private Dictionary<string, bool> unlockStatus = new Dictionary<string, bool>
    {
        { "Double Jump", true },
        { "Climbing", false }
    };

    public List<Unlock> Get()
    {
        List<Unlock> unlocks = new List<Unlock>();

        foreach (var unlock in unlockStatus)
        {
            unlocks.Add(new Unlock(unlock.Key, "Description for " + unlock.Key, 100, UnlockTypes.Ability, 10, 10) { IsPurchased = unlock.Value });
        }

        return unlocks;
    }

    public void Set(List<Unlock> unlocks)
    {
        unlockStatus.Clear();
        foreach (var unlock in unlocks)
        {
            unlockStatus[unlock.Name] = unlock.IsPurchased;
        }
    }

    private void OnEnable() { }
}
