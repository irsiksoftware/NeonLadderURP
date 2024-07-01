using UnityEngine;
using TMPro;

public class EnemyDefeatedManager : MonoBehaviour
{
    public TextMeshProUGUI enemyDefeatedCounterText;
    private int defeatedEnemiesCount = 0;

    private void Start()
    {
        enabled = false;
    }

    void Awake() { }

    public void InteractWithEntity(GameObject entity)
    {
        defeatedEnemiesCount++;
        UpdateEnemyDefeatedUI();
    }

    void UpdateEnemyDefeatedUI()
    {
        if (enemyDefeatedCounterText != null)
            enemyDefeatedCounterText.text = "Enemies Defeated: " + defeatedEnemiesCount;
        else
            Debug.LogWarning("EnemyDefeatedCounterText not set on " + gameObject.name);
    }
}
