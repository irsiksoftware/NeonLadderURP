using UnityEngine;
using UnityEngine.InputSystem;
using NeonLadder.UI;

namespace NeonLadder.Tests.Runtime.UI
{
    /// <summary>
    /// Shared mock classes for UI testing to avoid duplicate definitions
    /// </summary>

    /// <summary>
    /// Mock Loading3DController for testing navigation calls
    /// Inherits from the actual Loading3DController to ensure type compatibility
    /// But overrides critical methods to prevent real 3D model loading
    /// </summary>
    public class MockLoading3DController : Loading3DController
    {
        public int NextCallCount { get; private set; }
        public int PreviousCallCount { get; private set; }
        private bool interceptCalls = true;

        private void Awake()
        {
            // Disable the content database to prevent real model loading
            var field = typeof(Loading3DController).GetField("contentDatabase",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, null);
        }

        public new void NavigateToNextModel()
        {
            NextCallCount++;
            if (!interceptCalls)
            {
                base.NavigateToNextModel();
            }
            // When interceptCalls is true (default), we don't call base method
        }

        public new void NavigateToPreviousModel()
        {
            PreviousCallCount++;
            if (!interceptCalls)
            {
                base.NavigateToPreviousModel();
            }
            // When interceptCalls is true (default), we don't call base method
        }

        public void Reset()
        {
            NextCallCount = 0;
            PreviousCallCount = 0;
        }

        public void SetInterceptCalls(bool enabled)
        {
            interceptCalls = enabled;
        }

        public void IncrementNextCount()
        {
            NextCallCount++;
        }

        public void IncrementPreviousCount()
        {
            PreviousCallCount++;
        }
    }

    /// <summary>
    /// Mock InputAction that returns specified values for testing
    /// </summary>
    public class MockInputAction
    {
        public string name { get; }
        public bool enabled { get; set; } = true;
        private Vector2 currentValue;

        public MockInputAction(string actionName, Vector2 value)
        {
            name = actionName;
            currentValue = value;
        }

        public MockInputAction(string actionName)
        {
            name = actionName;
            currentValue = Vector2.zero;
        }

        public T ReadValue<T>() where T : struct
        {
            if (typeof(T) == typeof(Vector2))
            {
                return (T)(object)currentValue;
            }
            return default(T);
        }

        public void SetValue(Vector2 value)
        {
            currentValue = value;
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }
    }

    /// <summary>
    /// Simple mock PlayerInput component for testing
    /// Used for basic component existence tests rather than complex integration testing
    /// </summary>
    public class MockPlayerInput : MonoBehaviour
    {
        public bool WasDisabled { get; private set; }
        public bool WasEnabled { get; private set; }
        public bool IsActionMapEnabled { get; private set; } = true;

        // Simple tracking methods for tests that manually control state
        public void TrackDisable()
        {
            WasDisabled = true;
            IsActionMapEnabled = false;
        }

        public void TrackEnable()
        {
            WasEnabled = true;
            IsActionMapEnabled = true;
        }

        public void Reset()
        {
            WasDisabled = false;
            WasEnabled = false;
            IsActionMapEnabled = true;
        }
    }

    /// <summary>
    /// Mock InputActionAsset for testing
    /// </summary>
    public class MockInputActionAsset : ScriptableObject
    {
        public MockInputActionMap FindActionMap(string name)
        {
            if (name == "Player")
                return new MockInputActionMap(name);
            return null;
        }
    }

    /// <summary>
    /// Mock InputActionMap for testing
    /// </summary>
    public class MockInputActionMap
    {
        public string name { get; }
        public bool enabled { get; private set; } = true;

        public MockInputActionMap(string mapName)
        {
            name = mapName;
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }

        public MockInputAction FindAction(string actionName)
        {
            return new MockInputAction(actionName);
        }
    }
}