using NeonLadder.Mechanics.Stats;
using UnityEngine;

public class StaminaIncreaseStatManager : MonoBehaviour
{
    public Stamina Stamina;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Stamina.max += 1;
    }
}
