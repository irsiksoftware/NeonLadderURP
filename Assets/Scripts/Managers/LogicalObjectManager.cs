using NeonLadder.Debugging;
using NeonLadder.ProceduralGeneration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogicalObjectManager : MonoBehaviour
{
    #region Singleton

    private static LogicalObjectManager instance;
    public static LogicalObjectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LogicalObjectManager>();
                if (instance == null)
                {
                    Debugger.LogError(LogCategory.General, "No LogicalObjectManager found! Managers prefab may be missing from scene.");
                }
            }
            return instance;
        }
    }

    #endregion

    #region Configuration

    [Header("Barrier Control")]
    [SerializeField] private bool enableBarrierControl = true;
    [SerializeField] private string leftBarrierTag = "LeftBarrier";
    [SerializeField] private string rightBarrierTag = "RightBarrier";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    #endregion

    #region Private Fields

    private ProceduralPathTransitions _proceduralPathTransitions;
    private GameObject _leftBarrier;
    private GameObject _rightBarrier;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Ensure singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        InitializeReferences();

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Check current scene
        UpdateBarriersForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region Initialization

    private void InitializeReferences()
    {
        // Get ProceduralPathTransitions component
        _proceduralPathTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();

        if (_proceduralPathTransitions == null && enableDebugLogging)
        {
            Debugger.LogWarning(LogCategory.ProceduralGeneration,
                "[LogicalObjectManager] ProceduralPathTransitions not found on SceneTransitionManager");
        }
    }

    #endregion

    #region Scene Management

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (enableDebugLogging)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[LogicalObjectManager] Scene loaded: {scene.name}");
        }

        UpdateBarriersForCurrentScene();
    }

    private void UpdateBarriersForCurrentScene()
    {
        // Only manage barriers in the Start scene
        if (SceneManager.GetActiveScene().name != "Start")
        {
            return;
        }

        FindBarriers();
        UpdateBarrierStates();
    }

    #endregion

    #region Barrier Control

    private void FindBarriers()
    {
        // Find barriers by name first (most reliable)
        _leftBarrier = GameObject.Find("LeftBarrier");
        _rightBarrier = GameObject.Find("RightBarrier");

        // Fallback to tag-based search if configured
        if (_leftBarrier == null && !string.IsNullOrEmpty(leftBarrierTag))
        {
            _leftBarrier = GameObject.FindGameObjectWithTag(leftBarrierTag);
        }

        if (_rightBarrier == null && !string.IsNullOrEmpty(rightBarrierTag))
        {
            _rightBarrier = GameObject.FindGameObjectWithTag(rightBarrierTag);
        }

        if (enableDebugLogging)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[LogicalObjectManager] Barriers found - Left: {_leftBarrier != null}, Right: {_rightBarrier != null}");
        }
    }

    /// <summary>
    /// Update barrier states based on boss progression
    /// </summary>
    private void UpdateBarrierStates()
    {
        if (!enableBarrierControl || _proceduralPathTransitions == null)
        {
            // If barrier control is disabled or we can't access progression, enable both paths
            SetBarrierState(_leftBarrier, false);
            SetBarrierState(_rightBarrier, false);
            return;
        }

        // Get defeated bosses list
        var defeatedBosses = _proceduralPathTransitions.DefeatedBosses;

        // Check if we need to force a single path (7th boss or finale)
        bool shouldForceSinglePath = ShouldForceSinglePath(defeatedBosses);

        if (shouldForceSinglePath)
        {
            // When 6+ bosses are defeated (2 or fewer remain), we need to guide player down single path
            // This handles both 7th boss scenario and finale scenario
            bool forceLeftPath = DetermineFinalePathDirection();

            // Block opposite path to force player toward remaining boss(es)
            SetBarrierState(_leftBarrier, !forceLeftPath);  // Block left if forced path is right
            SetBarrierState(_rightBarrier, forceLeftPath);   // Block right if forced path is left

            if (enableDebugLogging)
            {
                int nonFinaleDefeated = defeatedBosses.Count(boss =>
                    !boss.Equals("Devil", System.StringComparison.OrdinalIgnoreCase) &&
                    !boss.Equals("Finale", System.StringComparison.OrdinalIgnoreCase));

                string openPath = forceLeftPath ? "LEFT" : "RIGHT";
                string scenario = nonFinaleDefeated >= 7 ? "finale" : "7th boss";

                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[LogicalObjectManager] Single path forced for {scenario} - Only {openPath} path available ({nonFinaleDefeated}/7 deadly sins defeated)");
            }
        }
        else if (_proceduralPathTransitions.IsPathsConverged)
        {
            // When paths are already converged (after defeating 6 bosses, before facing 7th)
            // Both paths should lead to the same boss, so keep both open
            SetBarrierState(_leftBarrier, false);
            SetBarrierState(_rightBarrier, false);

            if (enableDebugLogging)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    "[LogicalObjectManager] Paths converged - Both barriers disabled");
            }
        }
        else
        {
            // Normal gameplay - both paths available
            SetBarrierState(_leftBarrier, false);
            SetBarrierState(_rightBarrier, false);

            if (enableDebugLogging)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[LogicalObjectManager] Normal progression - Both paths open ({defeatedBosses.Count}/8 bosses defeated)");
            }
        }
    }

    /// <summary>
    /// Determine if we should force a single path route (when 2 or fewer bosses remain)
    /// This handles both the 7th boss scenario and the finale scenario
    /// </summary>
    private bool ShouldForceSinglePath(List<string> defeatedBosses)
    {
        // Count how many non-finale bosses are defeated
        int nonFinaleDefeated = defeatedBosses.Count(boss =>
            !boss.Equals("Devil", System.StringComparison.OrdinalIgnoreCase) &&
            !boss.Equals("Finale", System.StringComparison.OrdinalIgnoreCase));

        // Force single path when 6 or more deadly sins are defeated
        // This covers both 7th boss (6 defeated, 2 remaining) and finale (7 defeated, 1 remaining)
        return nonFinaleDefeated >= 6;
    }

    /// <summary>
    /// Determine which path should lead to finale based on seed
    /// </summary>
    private bool DetermineFinalePathDirection()
    {
        // Use the procedural system's seed to deterministically choose
        // This ensures consistency with the overall procedural generation
        if (_proceduralPathTransitions != null && !string.IsNullOrEmpty(_proceduralPathTransitions.CurrentSeed))
        {
            // Use simple hash of seed to determine direction
            int seedHash = _proceduralPathTransitions.CurrentSeed.GetHashCode();
            return (seedHash % 2) == 0; // Even = left, Odd = right
        }

        // Default to left path if no seed available
        return true;
    }

    /// <summary>
    /// Set the active state of a barrier
    /// </summary>
    private void SetBarrierState(GameObject barrier, bool isActive)
    {
        if (barrier != null)
        {
            barrier.SetActive(isActive);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually refresh barrier states
    /// </summary>
    public void RefreshBarriers()
    {
        UpdateBarriersForCurrentScene();
    }

    /// <summary>
    /// Get current barrier states
    /// </summary>
    public (bool leftBlocked, bool rightBlocked) GetBarrierStates()
    {
        return (_leftBarrier != null && _leftBarrier.activeSelf,
                _rightBarrier != null && _rightBarrier.activeSelf);
    }

    /// <summary>
    /// Force a specific barrier configuration (for testing)
    /// </summary>
    public void ForceBarrierConfiguration(bool blockLeft, bool blockRight)
    {
        FindBarriers();
        SetBarrierState(_leftBarrier, blockLeft);
        SetBarrierState(_rightBarrier, blockRight);

        if (enableDebugLogging)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[LogicalObjectManager] Forced barrier config - Left: {blockLeft}, Right: {blockRight}");
        }
    }

    #endregion
}
