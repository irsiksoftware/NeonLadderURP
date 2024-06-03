using NeonLadder.Mechanics.Stats;
using TMPro;
using UnityEngine;

public class PlayerHealthUIManager : MonoBehaviour
{

    [SerializeField]
    public Health playerHealth;

    public TextMeshProUGUI HealthCounterText;

    void Awake()
    {
    }

    private void Update()
    {
        HealthCounterText.text = $"Health {playerHealth.current}/{playerHealth.max}";
    }
}
