using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Steam;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public class DemoComplete : MonoBehaviour
    {
        [Header("Demo Completion Settings")]
        [SerializeField] private BoxCollider triggerCollider;
        [SerializeField] private float celebrationDuration = 5f;

        private ProceduralPathTransitions pathTransitions;

        private bool demoCompleted = false;
        private bool playerInTrigger = false;

        void Start()
        {
            // Get the box collider component
            if (triggerCollider == null)
                triggerCollider = GetComponent<BoxCollider>();

            // Ensure it's set as trigger
            triggerCollider.isTrigger = true;

            // Find the ProceduralPathTransitions component from the SceneTransitionManager in the scene
            var sceneTransitionManager = FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Single();
            if (sceneTransitionManager != null)
            {
                pathTransitions = sceneTransitionManager.GetComponent<ProceduralPathTransitions>();
                if (pathTransitions == null)
                {
                    Debug.LogError("[DemoComplete] ProceduralPathTransitions component not found on SceneTransitionManager!");
                }
                else
                {
                    Debug.Log("[DemoComplete] Successfully found ProceduralPathTransitions component");
                }
            }
            else
            {
                Debug.LogError("[DemoComplete] SceneTransitionManager not found in scene!");
            }

            // Disable the collider initially - only enable when final boss is defeated
            UpdateColliderState();
        }

        void Update()
        {
            // Update collider state based on final boss defeat status
            UpdateColliderState();
        }

        private void UpdateColliderState()
        {
            // Check if the final boss (Hara/Devil) has been defeated
            bool finalBossDefeated = IsFinalBossDefeated();

            // Enable collider only when final boss is defeated
            triggerCollider.enabled = finalBossDefeated;

            // Optional: Add visual feedback when collider becomes active
            if (finalBossDefeated && !triggerCollider.enabled)
            {
                Debug.Log("[DemoComplete] Final boss defeated! Demo completion trigger is now active.");
            }
        }

        private bool IsFinalBossDefeated()
        {
            // Check if "Hara" (the devil) is in the defeated bosses list from ProceduralPathTransitions
            if (pathTransitions == null) return false;

            return pathTransitions.DefeatedBosses.Contains("Hara") || pathTransitions.DefeatedBosses.Contains("Devil");
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player entered and demo hasn't been completed yet
            if (other.CompareTag("Player") && !demoCompleted && IsFinalBossDefeated())
            {
                Debug.Log("[DemoComplete] Player entered demo completion trigger!");

                playerInTrigger = true;
                StartDemoCompletion();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTrigger = false;
            }
        }

        private void StartDemoCompletion()
        {
            if (demoCompleted) return;

            demoCompleted = true;
            Debug.Log("[DemoComplete] Starting demo completion sequence!");

            // 1. Get player reference (singleton)
            var player = Game.Instance?.model?.Player;
            player.DisableZMovement();
            if (player == null)
            {
                Debug.LogError("[DemoComplete] Player not found!");
                return;
            }

            // 2. Disable player controls
            //var playerMediator = player.GetComponent<PlayerStateMediator>();
            //if (playerMediator != null)
            //{
            //    playerMediator.DisablePlayerActionMap();
            //    Debug.Log("[DemoComplete] Player controls disabled");
            //}

            // 3. Orient player to face right
            player.IsFacingLeft = false;
            player.Orient();

            // 4. Trigger celebration dance animation (locomotion_animation = 9044)
            var playerAnimator = player.transform.parent.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("locomotion_animation", 9044);
                Debug.Log("[DemoComplete] Victory dance animation started!");
            }

            // 5. Pop the demo completion achievement
            TriggerDemoAchievement();

            // 6. Start coroutine to freeze player movement during dance
            float extendedDuration = celebrationDuration * 2f;
            StartCoroutine(FreezeDuringDance(player, extendedDuration));
        }

        private IEnumerator FreezeDuringDance(Player player, float duration)
        {

            // Wait for dance duration
            yield return new WaitForSeconds(duration);
            // Now transition to credits
            TransitionToCredits();
        }

        private void TriggerDemoAchievement()
        {
            try
            {
                Debug.Log($"[DemoComplete] Attempting to trigger achievement: {Achievements.DEMO_LEVEL_COMPLETE}");

                // Use the SteamManager to unlock the demo completion achievement
                if (SteamManager.Instance != null)
                {
                    SteamManager.Instance.UnlockAchievement(Achievements.DEMO_LEVEL_COMPLETE.ToString());
                }
                else
                {
                    Debug.LogWarning("[DemoComplete] ❌ SteamManager instance not found - achievement not triggered (possibly running without Steam)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DemoComplete] ❌ EXCEPTION while triggering achievement: {e.Message}");
                Debug.LogError($"[DemoComplete] Stack trace: {e.StackTrace}");
            }
        }

        private void TransitionToCredits()
        {
            Debug.Log("[DemoComplete] Transitioning to Credits scene via SceneTransitionManager (Credits is excluded cutscene)");

            try
            {
                // Find the SceneTransitionManager (should be the same one we found in Start)
                var sceneTransitionManager = FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Single();
                if (sceneTransitionManager != null)
                {
                    // Credits is now marked as cutscene, so no spawn point handling will occur
                    sceneTransitionManager.TransitionToScene(Scenes.Core.Credits, SpawnPointType.Auto);
                    Debug.Log("[DemoComplete] Scene transition to Credits initiated successfully - fade effects included");
                    Debug.Log("[DemoComplete] AutoScrollText in Credits scene should handle transition to Title after scroll completes");
                }
                else
                {
                    Debug.LogError("[DemoComplete] SceneTransitionManager not found! Using fallback scene loading");
                    SceneManager.LoadScene(Scenes.Core.Credits);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DemoComplete] Failed to load credits scene: {e.Message}");
                // Fallback: transition to title screen
                TransitionToTitle();
            }
        }

        private void TransitionToTitle()
        {
                // Find the SceneTransitionManager (should be the same one we found in Start)
                var sceneTransitionManager = FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Single();
                if (sceneTransitionManager != null)
                {
                    // Credits is now marked as cutscene, so no spawn point handling will occur
                    sceneTransitionManager.TransitionToScene(Scenes.Core.Title);
                    Debug.Log("[DemoComplete] Scene transition to Credits initiated successfully - fade effects included");
                    Debug.Log("[DemoComplete] AutoScrollText in Credits scene should handle transition to Title after scroll completes");
                }
                else
                {
                    Debug.LogError("[DemoComplete] SceneTransitionManager not found! Using fallback scene loading");
                    SceneManager.LoadScene(Scenes.Core.Credits);
                }
        }

        // Optional: Gizmos for editor visualization
        private void OnDrawGizmosSelected()
        {
            if (triggerCollider != null)
            {
                Gizmos.color = IsFinalBossDefeated() ? Color.green : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
            }
        }
    }
}
