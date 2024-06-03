using UnityEngine;
using TMPro; // Make sure to include the TextMeshPro namespace

namespace NeonLadder.Gameplay
{
    public class GameTimer : MonoBehaviour
    {
        public static GameTimer Instance; // Singleton instance

        public TMP_Text timerText; // Reference to the TMP UI component
        public float timerDuration = 1800f; // Timer duration in seconds (30 minutes by default)
        private float timeRemaining; // Time remaining on the timer
        public bool isTimerRunning = false; // Control the timer's state

        private void Awake()
        {
            // Implement the Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject); // Prevent the timer from being destroyed on scene load
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            timeRemaining = timerDuration; // Initialize the timer
            isTimerRunning = true; // Start the timer
        }

        private void Update()
        {
            if (isTimerRunning)
            {
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime; // Decrease the timer
                    UpdateTimerDisplay();
                }
                else
                {
                    Debug.Log("Timer has ended!");
                    timeRemaining = 0;
                    isTimerRunning = false; // Stop the timer
                }
            }
        }

        private void UpdateTimerDisplay()
        {
            // Update the TMP UI component
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("The End: {0:00}:{1:00}", minutes, seconds);
        }

        // Method to start the timer externally
        public void StartTimer()
        {
            if (!isTimerRunning)
            {
                timeRemaining = timerDuration;
                isTimerRunning = true;
            }
        }

        // Method to stop the timer externally
        public void StopTimer()
        {
            isTimerRunning = false;
        }
    }
}