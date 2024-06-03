using NeonLadder.Mechanics.Stats;
using TMPro;
using UnityEngine;

public class PlayerStaminaUIManager : MonoBehaviour
{

    [SerializeField]
    public Stamina playerStamina;

    public TextMeshProUGUI StaminaCounterTest;

    void Awake()
    {
    }

    private void Update()
    {
        StaminaCounterTest.text = $"Stamina {playerStamina.current.ToString("F1")}/{playerStamina.max.ToString("F1")}";
    }
}
