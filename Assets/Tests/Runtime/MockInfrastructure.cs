using UnityEngine;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Core;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Tests.Runtime.SceneTransition
{
    /// <summary>
    /// Mock infrastructure for Unity component testing in scene transition system
    /// Provides controllable substitutes for Player, Animator, and other dependencies
    /// </summary>
    public static class MockInfrastructure
    {
        /// <summary>
        /// Creates a mock Player component with minimal functionality for testing
        /// </summary>
        public static MockPlayer CreateMockPlayer(GameObject gameObject)
        {
            return gameObject.AddComponent<MockPlayer>();
        }

        /// <summary>
        /// Creates a test GameObject with necessary components for spawn point testing
        /// </summary>
        public static GameObject CreateTestPlayerObject(string name = "TestPlayer")
        {
            var playerObject = new GameObject(name);
            
            // Add Rigidbody for physics
            var rigidbody = playerObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Add mock player component
            var mockPlayer = CreateMockPlayer(playerObject);
            
            // Add mock animator
            var animator = playerObject.AddComponent<Animator>();
            var mockAnimator = playerObject.AddComponent<MockAnimator>();
            
            return playerObject;
        }

        /// <summary>
        /// Creates a test spawn point configuration with specified parameters
        /// </summary>
        public static GameObject CreateTestSpawnPoint(string name, Vector3 position, Mechanics.Enums.SpawnPointType spawnType = Mechanics.Enums.SpawnPointType.Auto)
        {
            var spawnPointObject = new GameObject($"SpawnPoint_{name}");
            spawnPointObject.transform.position = position;
            
            var config = spawnPointObject.AddComponent<SpawnPointConfiguration>();
            // We'll need to set the spawn mode through reflection since it's private
            var spawnModeField = typeof(SpawnPointConfiguration).GetField("spawnMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spawnModeField?.SetValue(config, spawnType);
            
            return spawnPointObject;
        }

        /// <summary>
        /// Creates a test scene root with SceneTransitionPrefabRoot component
        /// </summary>
        public static GameObject CreateTestSceneRoot(string sceneName, SceneTransitionPrefabRoot.Orientation orientation = SceneTransitionPrefabRoot.Orientation.North)
        {
            var sceneRoot = new GameObject($"SceneRoot_{sceneName}");
            var prefabRoot = sceneRoot.AddComponent<SceneTransitionPrefabRoot>();
            
            // Set orientation through reflection
            var orientationField = typeof(SceneTransitionPrefabRoot).GetField("orientation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            orientationField?.SetValue(prefabRoot, orientation);
            
            return sceneRoot;
        }
    }

    /// <summary>
    /// Mock Player component for testing spawn point functionality
    /// </summary>
    public class MockPlayer : MonoBehaviour
    {
        public bool TeleportCalled { get; private set; }
        public Vector3 LastTeleportPosition { get; private set; }
        public int TeleportCallCount { get; private set; }

        /// <summary>
        /// Mock implementation of Player.Teleport method
        /// </summary>
        public void Teleport(Vector3 position)
        {
            TeleportCalled = true;
            LastTeleportPosition = position;
            TeleportCallCount++;
            transform.position = position;
        }

        /// <summary>
        /// Reset mock state for test isolation
        /// </summary>
        public void ResetMockState()
        {
            TeleportCalled = false;
            LastTeleportPosition = Vector3.zero;
            TeleportCallCount = 0;
        }

        /// <summary>
        /// Simulate player movement for testing
        /// </summary>
        public void SimulateMoveTo(Vector3 position)
        {
            transform.position = position;
        }
    }

    /// <summary>
    /// Mock Animator component for testing animation states
    /// </summary>
    public class MockAnimator : MonoBehaviour
    {
        public bool DanceAnimationCalled { get; private set; }
        public string LastTriggerSet { get; private set; }
        public bool IsInDanceState { get; private set; }

        private Animator unityAnimator;

        private void Awake()
        {
            unityAnimator = GetComponent<Animator>();
        }

        /// <summary>
        /// Mock animation trigger for boss victory dance
        /// </summary>
        public void PlayDanceAnimation()
        {
            DanceAnimationCalled = true;
            LastTriggerSet = "Dance";
            IsInDanceState = true;
        }

        /// <summary>
        /// Simulate animation completion
        /// </summary>
        public void CompleteDanceAnimation()
        {
            IsInDanceState = false;
        }

        /// <summary>
        /// Reset mock state
        /// </summary>
        public void ResetMockState()
        {
            DanceAnimationCalled = false;
            LastTriggerSet = null;
            IsInDanceState = false;
        }
    }

    /// <summary>
    /// Mock ProceduralSceneLoader for testing scene loading
    /// </summary>
    public class MockProceduralSceneLoader : MonoBehaviour
    {
        public bool LoadSceneCalled { get; private set; }
        public string LastSceneLoaded { get; private set; }
        public bool SimulateLoadSuccess { get; set; } = true;

        /// <summary>
        /// Mock scene loading method
        /// </summary>
        public bool LoadScene(string sceneName)
        {
            LoadSceneCalled = true;
            LastSceneLoaded = sceneName;
            return SimulateLoadSuccess;
        }

        /// <summary>
        /// Reset mock state
        /// </summary>
        public void ResetMockState()
        {
            LoadSceneCalled = false;
            LastSceneLoaded = null;
            SimulateLoadSuccess = true;
        }
    }

    /// <summary>
    /// Mock Game.Instance.model for testing player references
    /// </summary>
    public class MockGameModel
    {
        public Player Player { get; set; }
        public MockPlayer MockPlayer { get; set; }

        public MockGameModel(MockPlayer mockPlayer)
        {
            MockPlayer = mockPlayer;
            // Player would normally be the real Player component
            // For testing, we use the MockPlayer
        }
    }
}