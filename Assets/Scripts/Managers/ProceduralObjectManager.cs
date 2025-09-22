using NeonLadder.Debugging;
using UnityEngine;

public class ProceduralObjectManager : MonoBehaviour
{
    #region Singleton

    private static ProceduralObjectManager instance;
    public static ProceduralObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ProceduralObjectManager>();
                if (instance == null)
                {
                    Debugger.LogError(LogCategory.General, "No SceneTransitionManager found! Managers prefab may be missing from scene.");
                }
            }
            return instance;
        }
    }

    #endregion
}
