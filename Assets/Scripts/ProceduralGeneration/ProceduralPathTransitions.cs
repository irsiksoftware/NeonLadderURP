using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// ProceduralPathTransitions Component with Dynamic Boss Pool System
    /// Manages seed-based boss selection with shrinking pools as bosses are defeated
    ///
    /// Goal: Create a deterministic yet dynamic procedural boss selection system that adapts as players progress
    ///
    /// UNITY EDITOR VERIFICATION CHECKLIST:
    /// =====================================
    ///
    /// 1. START SCENE SETUP:
    ///    □ Open Start.unity scene
    ///    □ Find the two exit triggers (left and right paths)
    ///    □ Verify both triggers have SceneTransitionTrigger component with:
    ///       - destinationType = Procedural
    ///       - enableDebugLogs = true (for testing)
    ///    □ Trigger names should contain "left" or "right" for path detection
    ///    □ Each trigger should have SpawnPointConfiguration set appropriately
    ///
    /// 2. SCENETRANSITIONMANAGER SETUP:
    ///    □ Find SceneTransitionManager GameObject (usually under Managers)
    ///    □ Verify it has ProceduralPathTransitions component attached
    ///    □ Check ProceduralPathTransitions settings:
    ///       - Enable Debug Logging = true (for testing)
    ///       - Show Path Prediction = true
    ///       - Defeated Bosses list (should be empty for new run)
    ///
    /// 3. CONNECTION SCENE VERIFICATION (for each Connection1 and Connection2):
    ///    □ Open any [Boss]_Connection1.unity scene
    ///    □ Verify FORWARD exit trigger:
    ///       - destinationType = Manual
    ///       - overrideSceneName = [Boss]_Connection2
    ///    □ Verify BACKWARD exit trigger:
    ///       - destinationType = Manual
    ///       - overrideSceneName = Start
    ///    □ Check SpawnPointConfiguration components exist with proper directions
    ///
    /// 4. BOSS ARENA VERIFICATION:
    ///    □ Boss defeat should trigger transition to BossDefeated cutscene
    ///    □ BossDefeated cutscene should transition to Staging
    ///    □ Boss name should be tracked in defeatedBosses list
    ///
    /// 5. TESTING THE FLOW:
    ///    □ Start Play Mode in Start scene
    ///    □ Walk to left or right exit trigger
    ///    □ Console should show: "[PROCEDURAL] Path LEFT/RIGHT selected: [Boss Name]"
    ///    □ Console should show: "Routing through connection scene: [Boss]_Connection1"
    ///    □ Verify scene loads to correct Connection1 scene
    ///    □ Continue through Connection2 to Boss arena
    ///    □ After defeating boss, verify return to Staging
    ///    □ Check defeatedBosses list updated in ProceduralPathTransitions
    ///
    /// 6. DEBUGGING TIPS:
    ///    - Use Debug.Log statements to track path selection
    ///    - Check SceneTransitionManager logs for spawn point issues
    ///    - Verify all scenes are added to Build Settings
    ///    - Test with specific seeds for reproducible results
    ///
    /// </summary>
    public class ProceduralPathTransitions : MonoBehaviour
    {
        #region Configuration

        [Header("Seed Configuration")]
        [SerializeField] private string gameSeed = "";
        [SerializeField] private bool useRandomSeed = true;

        [Header("Boss Pool Configuration")]
        [SerializeField] private List<string> defeatedBosses = new List<string>();

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showPathPrediction = true;

        #endregion

        #region Private Fields

        private System.Random _seededRandom;
        private PathGenerator _pathGenerator;
        private List<BossLocationData> _leftPathBosses;
        private List<BossLocationData> _rightPathBosses;
        private List<BossLocationData> _availableBosses;
        private bool _isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current game seed being used for procedural generation
        /// </summary>
        public string CurrentSeed => gameSeed;

        /// <summary>
        /// List of defeated boss identifiers
        /// </summary>
        public List<string> DefeatedBosses => new List<string>(defeatedBosses);

        /// <summary>
        /// Returns true when only one boss remains and both paths converge
        /// </summary>
        public bool IsPathsConverged => _availableBosses?.Count <= 1;

        /// <summary>
        /// Gets the current available boss pool
        /// </summary>
        public List<BossLocationData> AvailableBosses => new List<BossLocationData>(_availableBosses ?? new List<BossLocationData>());

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            LogSeedToConsole();
            if (showPathPrediction)
            {
                DisplayPathPrediction();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the procedural path transitions component
        /// </summary>
        private void InitializeComponent()
        {
            if (_isInitialized) return;

            // Try to get seed from main game system first (integrated approach)
            string mainGameSeed = GetMainGameSeed();
            if (!string.IsNullOrEmpty(mainGameSeed))
            {
                // Derive path seed from main game seed for consistency
                gameSeed = DeriveBossPathSeed(mainGameSeed);
                useRandomSeed = false;

                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Using derived seed from main game: {mainGameSeed} -> {gameSeed}");
                }
            }
            else if (useRandomSeed || string.IsNullOrEmpty(gameSeed))
            {
                // Fallback to random generation if main game seed unavailable
                gameSeed = GenerateRandomSeed();

                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Main game seed unavailable, generated fallback: {gameSeed}");
                }
            }

            // Initialize random generator with seed
            _seededRandom = new System.Random(gameSeed.GetHashCode());
            _pathGenerator = new PathGenerator();

            // Initialize boss pools
            InitializeBossPools();

            _isInitialized = true;
        }

        /// <summary>
        /// Initialize boss pools with left/right path separation
        /// </summary>
        private void InitializeBossPools()
        {
            var allBosses = BossLocationData.Locations.ToList();

            // Split bosses into left and right paths as specified in PBI
            _leftPathBosses = new List<BossLocationData>
            {
                allBosses.First(b => b.Boss == "Pride"),    // Cathedral
                allBosses.First(b => b.Boss == "Greed"),   // Vault
                allBosses.First(b => b.Boss == "Lust"),    // Garden
                allBosses.First(b => b.Boss == "Sloth")    // Lounge
            };

            _rightPathBosses = new List<BossLocationData>
            {
                allBosses.First(b => b.Boss == "Wrath"),   // Necropolis
                allBosses.First(b => b.Boss == "Envy"),    // Mirage
                allBosses.First(b => b.Boss == "Gluttony"), // Banquet
                allBosses.First(b => b.Boss == "Devil")    // Finale
            };

            // Initialize available bosses (remove defeated ones)
            UpdateAvailableBosses();
        }

        /// <summary>
        /// Get seed from main game system if available
        /// </summary>
        private string GetMainGameSeed()
        {
            try
            {
                // Check if Game instance exists and has a procedural map with seed
                if (NeonLadder.Mechanics.Controllers.Game.Instance?.ProceduralMap?.Seed != null)
                {
                    return NeonLadder.Mechanics.Controllers.Game.Instance.ProceduralMap.Seed;
                }
            }
            catch (System.Exception ex)
            {
                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Error accessing main game seed: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Derive a boss path seed from the main game seed
        /// Ensures deterministic boss progression based on main seed
        /// </summary>
        private string DeriveBossPathSeed(string mainGameSeed)
        {
            if (string.IsNullOrEmpty(mainGameSeed))
            {
                return GenerateRandomSeed();
            }

            // Use first 6 characters, convert to uppercase for consistency with boss path format
            string derived = mainGameSeed.Length >= 6
                ? mainGameSeed.Substring(0, 6).ToUpper()
                : (mainGameSeed + "000000").Substring(0, 6).ToUpper();

            // Ensure only valid characters (A-Z, 0-9)
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new System.Text.StringBuilder(6);

            foreach (char c in derived)
            {
                if (validChars.Contains(char.ToUpper(c)))
                {
                    result.Append(char.ToUpper(c));
                }
                else
                {
                    // Replace invalid characters with deterministic alternatives based on position
                    int index = (int)c % validChars.Length;
                    result.Append(validChars[index]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Generate a random seed string (fallback when main game seed unavailable)
        /// </summary>
        private string GenerateRandomSeed()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Boss Pool Management

        /// <summary>
        /// Update available bosses by removing defeated ones
        /// </summary>
        private void UpdateAvailableBosses()
        {
            _availableBosses = BossLocationData.Locations
                .Where(boss => !defeatedBosses.Contains(boss.Boss))
                .ToList();
        }

        /// <summary>
        /// Mark a boss as defeated and remove from selection pool
        /// </summary>
        public void MarkBossAsDefeated(string bossIdentifier)
        {
            if (!defeatedBosses.Contains(bossIdentifier))
            {
                defeatedBosses.Add(bossIdentifier);
                UpdateAvailableBosses();

                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Boss defeated: {bossIdentifier}. Remaining: {_availableBosses.Count}");
                }

                // Check for path convergence
                if (IsPathsConverged)
                {
                    if (enableDebugLogging)
                    {
                        Debugger.LogInformation(LogCategory.ProceduralGeneration, "[PROCEDURAL] Paths converged - Final Boss mode activated");
                    }
                }
            }
        }

        /// <summary>
        /// Get available bosses for a specific path (left or right)
        /// </summary>
        public List<BossLocationData> GetAvailableBossesForPath(bool isLeftPath)
        {
            var pathBosses = isLeftPath ? _leftPathBosses : _rightPathBosses;
            return pathBosses.Where(boss => _availableBosses.Contains(boss)).ToList();
        }

        #endregion

        #region Boss Selection

        /// <summary>
        /// Select next boss based on path direction and current game state
        /// </summary>
        public BossLocationData SelectNextBoss(bool isLeftPath)
        {
            if (!_isInitialized)
            {
                InitializeComponent();
            }

            // If paths converged, return the final boss regardless of direction
            if (IsPathsConverged)
            {
                return _availableBosses.FirstOrDefault();
            }

            // Get available bosses for the chosen path
            var availableForPath = GetAvailableBossesForPath(isLeftPath);

            // If chosen path is exhausted, use the other path
            if (availableForPath.Count == 0)
            {
                availableForPath = GetAvailableBossesForPath(!isLeftPath);
            }

            // If no bosses available, return null
            if (availableForPath.Count == 0)
            {
                return null;
            }

            // Use seeded random to select deterministically
            var selectedIndex = _seededRandom.Next(availableForPath.Count);
            var selectedBoss = availableForPath[selectedIndex];

            if (enableDebugLogging)
            {
                string pathDirection = IsPathsConverged ? "CONVERGED" : (isLeftPath ? "LEFT" : "RIGHT");
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Path {pathDirection} selected: {selectedBoss.DisplayName} ({selectedBoss.Boss})");
            }

            return selectedBoss;
        }

        /// <summary>
        /// Preview what boss would be selected for each path without actually selecting
        /// </summary>
        public (BossLocationData leftChoice, BossLocationData rightChoice) PreviewNextChoices()
        {
            if (!_isInitialized)
            {
                InitializeComponent();
            }

            // If converged, both paths lead to same boss
            if (IsPathsConverged)
            {
                var finalBoss = _availableBosses.FirstOrDefault();
                return (finalBoss, finalBoss);
            }

            // Create temporary random instance with same seed for preview
            var tempRandom = new System.Random(gameSeed.GetHashCode());

            // Preview left path
            var leftAvailable = GetAvailableBossesForPath(true);
            if (leftAvailable.Count == 0)
            {
                leftAvailable = GetAvailableBossesForPath(false);
            }
            var leftChoice = leftAvailable.Count > 0 ? leftAvailable[tempRandom.Next(leftAvailable.Count)] : null;

            // Reset temp random for right path
            tempRandom = new System.Random(gameSeed.GetHashCode());
            var rightAvailable = GetAvailableBossesForPath(false);
            if (rightAvailable.Count == 0)
            {
                rightAvailable = GetAvailableBossesForPath(true);
            }
            var rightChoice = rightAvailable.Count > 0 ? rightAvailable[tempRandom.Next(rightAvailable.Count)] : null;

            return (leftChoice, rightChoice);
        }

        #endregion

        #region Logging and Visualization

        /// <summary>
        /// Log seed to console with specified format
        /// </summary>
        private void LogSeedToConsole()
        {
            string logMessage = $"[SEED: {gameSeed}] - Run started";

            if (enableDebugLogging)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration, logMessage);
            }
        }

        /// <summary>
        /// Display path prediction showing potential next choices
        /// </summary>
        private void DisplayPathPrediction()
        {
            if (!_isInitialized) return;

            var (leftChoice, rightChoice) = PreviewNextChoices();

            if (IsPathsConverged)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Path Prediction - FINAL BOSS: {leftChoice?.DisplayName ?? "None"}");
            }
            else
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Path Prediction - LEFT: {leftChoice?.DisplayName ?? "None"}, RIGHT: {rightChoice?.DisplayName ?? "None"}");
            }
        }

        /// <summary>
        /// Get path tree visualization as string
        /// </summary>
        public string GetPathTreeVisualization()
        {
            if (!_isInitialized) return "Component not initialized";

            var (leftChoice, rightChoice) = PreviewNextChoices();

            if (IsPathsConverged)
            {
                return $"[SEED: {gameSeed}] - Final Boss ({defeatedBosses.Count}/8 defeated):\n" +
                       $"START\n" +
                       $"└─[CONVERGED]→ {leftChoice?.DisplayName ?? "None"}";
            }
            else
            {
                return $"[SEED: {gameSeed}] - Initial State ({defeatedBosses.Count}/8 defeated):\n" +
                       $"START\n" +
                       $"├─[LEFT]─→ {leftChoice?.DisplayName ?? "Path Exhausted"}\n" +
                       $"└─[RIGHT]→ {rightChoice?.DisplayName ?? "Path Exhausted"}";
            }
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Static helper to mark a boss as defeated from anywhere in the game
        /// Useful for boss death events to update the procedural system
        /// </summary>
        public static void MarkBossDefeated(string bossName)
        {
            // Find the ProceduralPathTransitions instance
            var sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
            if (sceneTransitionManager == null)
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Cannot mark boss defeated - SceneTransitionManager not found");
                return;
            }

            var pathTransitions = sceneTransitionManager.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions == null)
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Cannot mark boss defeated - ProceduralPathTransitions not found");
                return;
            }

            // Mark the boss as defeated
            pathTransitions.MarkBossAsDefeated(bossName);

            Debugger.LogInformation(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Boss '{bossName}' marked as defeated via static helper");
        }

        /// <summary>
        /// Get the current procedural state from anywhere
        /// </summary>
        public static ProceduralPathState GetGlobalState()
        {
            var sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
            if (sceneTransitionManager == null) return null;

            var pathTransitions = sceneTransitionManager.GetComponent<ProceduralPathTransitions>();
            return pathTransitions?.GetCurrentState();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Reset the component with a new seed
        /// </summary>
        public void ResetWithNewSeed(string newSeed = null)
        {
            if (!string.IsNullOrEmpty(newSeed))
            {
                gameSeed = newSeed;
                useRandomSeed = false;
            }
            else
            {
                useRandomSeed = true;
            }

            defeatedBosses.Clear();
            _isInitialized = false;
            InitializeComponent();
            LogSeedToConsole();
        }

        /// <summary>
        /// Get current game state for serialization
        /// </summary>
        public ProceduralPathState GetCurrentState()
        {
            return new ProceduralPathState
            {
                Seed = gameSeed,
                DefeatedBosses = new List<string>(defeatedBosses),
                IsConverged = IsPathsConverged
            };
        }

        /// <summary>
        /// Load game state from serialization
        /// </summary>
        public void LoadState(ProceduralPathState state)
        {
            gameSeed = state.Seed;
            defeatedBosses = new List<string>(state.DefeatedBosses);
            useRandomSeed = false;
            _isInitialized = false;
            InitializeComponent();
        }

        #endregion
    }

    /// <summary>
    /// Serializable state for ProceduralPathTransitions
    /// </summary>
    [Serializable]
    public class ProceduralPathState
    {
        public string Seed;
        public List<string> DefeatedBosses;
        public bool IsConverged;
    }
}