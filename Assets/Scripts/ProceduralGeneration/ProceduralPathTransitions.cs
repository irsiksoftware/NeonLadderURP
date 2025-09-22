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
        [SerializeField] private int selectionCounter = 0; // Tracks how many times bosses have been selected

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showPathPrediction = true;

        [Header("Testing Settings")]
        [Tooltip("Skip connection scenes and go directly to boss arenas for faster testing")]
        [SerializeField] private bool fastForwardConnections = false;

        private bool originalDebugLoggingState; // Store original state for restoration

        #endregion

        #region Private Fields

        private System.Random _seededRandom;
        private PathGenerator _pathGenerator;
        private List<BossLocationData> _leftPathBosses;
        private List<BossLocationData> _rightPathBosses;
        private List<BossLocationData> _availableBosses;
        private List<BossLocationData> _orderedBossSequence; // Deterministic boss order for this run
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

        /// <summary>
        /// Whether to fast forward connections for testing (skip connection scenes, go directly to boss arenas)
        /// </summary>
        public bool FastForwardConnections => fastForwardConnections;

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
        /// Initialize boss pools - all bosses can now be selected from either direction
        /// </summary>
        private void InitializeBossPools()
        {
            var allBosses = BossLocationData.Locations.ToList();

            // NEW: All bosses are now available from both paths for truly dynamic runs
            // This change enables procedural selection across all bosses regardless of path
            _leftPathBosses = allBosses.ToList();
            _rightPathBosses = allBosses.ToList();

            // Generate deterministic boss order for this run (excluding Devil who is always last)
            GenerateOrderedBossSequence();

            // Initialize available bosses (remove defeated ones)
            UpdateAvailableBosses();
        }

        /// <summary>
        /// Generate a deterministic boss order for this run using the seed
        /// </summary>
        private void GenerateOrderedBossSequence()
        {
            // Get all 7 Deadly Sin bosses (excluding Devil who is always last)
            var sevenSins = BossLocationData.Locations
                .Where(boss => boss.Boss != "Devil")
                .ToList();

            // Create deterministic random with seed
            var random = new System.Random(gameSeed.GetHashCode());

            // Shuffle the 7 sins to create our ordered sequence for this run
            _orderedBossSequence = new List<BossLocationData>();
            var tempList = sevenSins.ToList();

            while (tempList.Count > 0)
            {
                int index = random.Next(tempList.Count);
                _orderedBossSequence.Add(tempList[index]);
                tempList.RemoveAt(index);
            }

            if (enableDebugLogging)
            {
                var sequence = string.Join(" → ", _orderedBossSequence.Select(b => b.Boss));
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Boss sequence for this run: {sequence} → Devil");
            }
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

                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"MarkBossAsDefeated called with: '{bossIdentifier}'");
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Current defeated list: [{string.Join(", ", defeatedBosses)}]");
                }

                UpdateAvailableBosses();

                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Boss defeated: {bossIdentifier}. Remaining: {_availableBosses.Count}. Total defeated: {defeatedBosses.Count}");

                    // Debug: Show which bosses are still available
                    var availableNames = _availableBosses.Select(b => $"{b.Identifier}({b.Boss})").ToArray();
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Available bosses: [{string.Join(", ", availableNames)}]");
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
            else
            {
                if (enableDebugLogging)
                {
                    Debugger.LogWarning(LogCategory.ProceduralGeneration, $"[PROCEDURAL] Boss '{bossIdentifier}' was already marked as defeated!");
                }
            }
        }

        /// <summary>
        /// Get available bosses for a specific path (left or right)
        /// With new bidirectional scenes, all bosses are available from both paths
        /// IMPORTANT: Finale boss is reserved for last and only available when it's the only boss remaining
        /// </summary>
        public List<BossLocationData> GetAvailableBossesForPath(bool isLeftPath)
        {
            // Filter out the Finale boss unless it's the only one left
            var availableNonFinale = _availableBosses
                .Where(boss => !boss.Boss.Equals("Devil", System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If there are still non-finale bosses available, return only those
            if (availableNonFinale.Count > 0)
            {
                return availableNonFinale;
            }

            // If only finale boss is left, return it
            return _availableBosses.ToList();
        }

        #endregion

        #region Boss Selection

        /// <summary>
        /// Select next boss based on path direction and current game state
        /// Uses dynamic selection that advances through available bosses deterministically
        /// </summary>
        public BossLocationData SelectNextBoss(bool isLeftPath)
        {
            // Get both choices using consistent logic, then return the requested path
            var (leftChoice, rightChoice) = SelectNextChoices();

            var selectedBoss = isLeftPath ? leftChoice : rightChoice;

            if (enableDebugLogging && selectedBoss != null)
            {
                string pathDirection = IsPathsConverged ? "CONVERGED" : (!isLeftPath ? "LEFT" : "RIGHT");
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Path {pathDirection} selected: {selectedBoss.DisplayName} ({selectedBoss.Boss})");
            }

            return selectedBoss;
        }

        /// <summary>
        /// Select next boss choices for both paths simultaneously to ensure they're different
        /// This ensures left and right paths get different bosses on the same visit
        /// </summary>
        public (BossLocationData leftChoice, BossLocationData rightChoice) SelectNextChoices()
        {
            if (!_isInitialized)
            {
                InitializeComponent();
            }

            // Get remaining bosses from our ordered sequence (excluding defeated ones)
            var remainingBosses = _orderedBossSequence
                .Where(boss => !defeatedBosses.Contains(boss.Boss))
                .ToList();

            if (enableDebugLogging)
            {
                var remaining = string.Join(", ", remainingBosses.Select(b => b.Boss));
                var defeated = string.Join(", ", defeatedBosses);
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Defeated: [{defeated}]");
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Remaining: [{remaining}]");
            }

            // Add Devil at the end if needed
            if (remainingBosses.Count == 0)
            {
                var devil = BossLocationData.Locations.FirstOrDefault(b => b.Boss == "Devil");
                if (devil != null && !defeatedBosses.Contains("Devil"))
                {
                    return (devil, devil); // Force final boss
                }
                return (null, null); // All bosses defeated
            }

            // If only 2 bosses left, converge paths (block one path)
            if (remainingBosses.Count == 2)
            {
                var finalChoice = remainingBosses[0]; // Force first remaining boss
                if (enableDebugLogging)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Paths converged - Final choice: {finalChoice.Boss}");
                }
                return (finalChoice, finalChoice);
            }

            // If only 1 boss left before Devil, force it
            if (remainingBosses.Count == 1)
            {
                var lastBoss = remainingBosses[0];
                return (lastBoss, lastBoss);
            }

            // Normal case: Show next 2 bosses from our ordered sequence
            var leftChoice = remainingBosses[0];
            var rightChoice = remainingBosses[1];

            if (enableDebugLogging)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Remaining bosses: {remainingBosses.Count}, Defeated: {defeatedBosses.Count}");
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Path choices: LEFT={rightChoice.Boss}, RIGHT={leftChoice.Boss}");
            }

            return (leftChoice, rightChoice);
        }

        /// <summary>
        /// Preview what boss would be selected for each path without actually selecting
        /// Uses the same dynamic logic as SelectNextBoss for consistent previews
        /// </summary>
        public (BossLocationData leftChoice, BossLocationData rightChoice) PreviewNextChoices()
        {
            // Use exact same logic as SelectNextChoices for consistent preview
            return SelectNextChoices();
        }

        /// <summary>
        /// Shuffle available bosses deterministically based on seed
        /// </summary>
        private List<BossLocationData> ShuffleAvailableBosses(List<BossLocationData> bosses, int seed)
        {
            var shuffled = new List<BossLocationData>(bosses);
            var random = new System.Random(seed);

            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            return shuffled;
        }

        /// <summary>
        /// Get two different bosses from the pool for left and right paths
        /// </summary>
        private (BossLocationData leftBoss, BossLocationData rightBoss) GetDifferentBossesFromPool(List<BossLocationData> shuffledPool, int seed)
        {
            if (shuffledPool.Count == 0)
                return (null, null);

            if (shuffledPool.Count == 1)
                return (shuffledPool[0], shuffledPool[0]); // Only one boss left

            // Take first two from shuffled pool to ensure they're different
            return (shuffledPool[0], shuffledPool[1]);
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
            selectionCounter = 0; // Reset selection counter for consistent deterministic behavior
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

        /// <summary>
        /// Temporarily disable debug logging for performance-critical operations
        /// </summary>
        public void DisableDebugLoggingTemporarily()
        {
            originalDebugLoggingState = enableDebugLogging;
            enableDebugLogging = false;
        }

        /// <summary>
        /// Restore debug logging to its original state
        /// </summary>
        public void RestoreDebugLogging()
        {
            enableDebugLogging = originalDebugLoggingState;
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