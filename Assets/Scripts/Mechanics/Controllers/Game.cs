using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Models;
using NeonLadder.ProceduralGeneration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Constants.Minimap = PathGenerator.GenerateIndentedText(PathGenerator.GeneratePaths()); //temp minimap
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

        public void DestroyGameInstance()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

    }
}
