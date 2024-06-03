using UnityEngine;
using TMPro;

public class EnemyDefeatedManager : InteractableEntitiesManager
{
    public static EnemyDefeatedManager Instance { get; private set; }

    public TextMeshProUGUI enemyDefeatedCounterText;
    private int defeatedEnemiesCount = 0;

    new void Awake()
    {
        // If there is an instance, and it's not me, destroy myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public override void InteractWithEntity(GameObject entity)
    {
        base.InteractWithEntity(entity);
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
