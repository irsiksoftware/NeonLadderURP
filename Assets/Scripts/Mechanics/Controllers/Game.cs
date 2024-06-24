using NeonLadder.Core;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// This class exposes the game model in the inspector, and ticks the
    /// simulation.
    /// </summary> 
    public class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }

        public PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Instance == this) Simulation.Tick();
        }

        // Method to enable all first-level children except those with the tag "PauseMenu" which are used throughout the game.
        public void EnableAllFirstLevelChildren()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.tag != "PauseMenu")
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
}
