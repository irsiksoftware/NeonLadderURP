using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Testing
{
    /// <summary>
    /// Test script to verify fast forward connections functionality
    /// Attach to any GameObject to test the fast forward feature
    /// </summary>
    public class FastForwardTest : MonoBehaviour
    {
        [Header("Fast Forward Testing")]
        [SerializeField] private bool testOnStart = true;

        private void Start()
        {
            if (testOnStart)
            {
                TestFastForwardFeature();
            }
        }

        [ContextMenu("Test Fast Forward Feature")]
        public void TestFastForwardFeature()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[FastForwardTest] Testing Fast Forward Connections feature...");

            var sceneTransitionManager = SceneTransitionManager.Instance;
            if (sceneTransitionManager == null)
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[FastForwardTest] SceneTransitionManager not found!");
                return;
            }

            var pathTransitions = sceneTransitionManager.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions == null)
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[FastForwardTest] ProceduralPathTransitions component not found!");
                return;
            }

            // Test both states
            TestFastForwardState(pathTransitions, false); // Normal mode
            TestFastForwardState(pathTransitions, true);  // Fast forward mode

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[FastForwardTest] Fast Forward testing completed!");
        }

        private void TestFastForwardState(ProceduralPathTransitions pathTransitions, bool fastForwardEnabled)
        {
            // Temporarily set the fast forward state for testing
            var fastForwardField = typeof(ProceduralPathTransitions).GetField("fastForwardConnections",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (fastForwardField != null)
            {
                // Store original value
                bool originalValue = pathTransitions.FastForwardConnections;

                // Set test value
                fastForwardField.SetValue(pathTransitions, fastForwardEnabled);

                // Test boss selection
                var selectedBoss = pathTransitions.SelectNextBoss(true); // Test left path

                if (selectedBoss != null)
                {
                    // Simulate what SceneTransitionTrigger would do
                    string destination = SimulateDestinationSelection(pathTransitions, selectedBoss, true);

                    string mode = fastForwardEnabled ? "FAST FORWARD" : "NORMAL";
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[FastForwardTest] {mode} Mode - Selected Boss: {selectedBoss.DisplayName}, Destination: {destination}");

                    // Validate destination format
                    bool isExpectedFormat = fastForwardEnabled
                        ? destination == selectedBoss.Identifier  // Should be boss arena directly
                        : destination.Contains("_Connection1");   // Should be connection scene

                    if (isExpectedFormat)
                    {
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            $"[FastForwardTest] ✓ PASS: {mode} destination format is correct");
                    }
                    else
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            $"[FastForwardTest] ✗ FAIL: {mode} destination format is incorrect");
                    }
                }
                else
                {
                    Debugger.LogWarning(LogCategory.ProceduralGeneration,
                        $"[FastForwardTest] No boss selected for testing (all may be defeated)");
                }

                // Restore original value
                fastForwardField.SetValue(pathTransitions, originalValue);
            }
            else
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[FastForwardTest] Could not access fastForwardConnections field for testing");
            }
        }

        /// <summary>
        /// Simulate the destination selection logic from SceneTransitionTrigger
        /// </summary>
        private string SimulateDestinationSelection(ProceduralPathTransitions pathTransitions, BossLocationData selectedBoss, bool isLeftPath)
        {
            if (pathTransitions.FastForwardConnections)
            {
                // Fast forward: Route directly to boss arena
                return selectedBoss.Identifier;
            }
            else
            {
                // Normal flow: Route to connection scene
                string pathFolder = isLeftPath ? "Left" : "Right";
                return $"{pathFolder}/{selectedBoss.Identifier}_Connection1";
            }
        }

        [ContextMenu("Toggle Fast Forward (Debug)")]
        public void ToggleFastForward()
        {
            if (!Application.isPlaying)
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration,
                    "[FastForwardTest] Can only toggle fast forward during play mode");
                return;
            }

            var pathTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions != null)
            {
                var fastForwardField = typeof(ProceduralPathTransitions).GetField("fastForwardConnections",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fastForwardField != null)
                {
                    bool currentValue = pathTransitions.FastForwardConnections;
                    fastForwardField.SetValue(pathTransitions, !currentValue);

                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[FastForwardTest] Fast Forward toggled: {!currentValue}");
                }
            }
        }
    }
}