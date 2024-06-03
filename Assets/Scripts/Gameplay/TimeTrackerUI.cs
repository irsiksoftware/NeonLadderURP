using TMPro;
using UnityEngine;

namespace NeonLadder.Gameplay
{
    public class TimeTrackerUI : MonoBehaviour
    {
        public TextMeshProUGUI timeRemainingText;
        public TextMeshProUGUI timeRemainingLabel;

        public void Awake()
        {
            timeRemainingLabel.enabled = false;
            timeRemainingText.enabled = false;
        }

        public void UpdateTimeRemaining(float timeRemaining)
        {
            timeRemainingLabel.enabled = true;
            timeRemainingText.enabled = true;
            timeRemainingText.text = timeRemaining.ToString("F1") + "s";

            if (timeRemaining <= 0)
            {
                timeRemainingLabel.enabled = false;
                timeRemainingText.enabled = false;
            }
        }
    }
}
