using System.Collections.Generic;
using UnityEngine;

public class PortalSpawnController : MonoBehaviour
{
    public List<GameObject> Prefabs = new List<GameObject>();
    public float SpawnIntervalInSeconds = 5f;
    public float PortalLifetimeInSeconds = 0f;

    private float spawnTimer = 0f;
    private float lifetimeTimer = 0f;
    private bool isSpawning = true;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = SpawnIntervalInSeconds;
        lifetimeTimer = PortalLifetimeInSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawning)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnRandomPrefabFromPortal();
                spawnTimer = SpawnIntervalInSeconds;
            }

            if (PortalLifetimeInSeconds > 0)
            {
                lifetimeTimer -= Time.deltaTime;
                if (lifetimeTimer <= 0f)
                {
                    DespawnPortal();
                }
            }
        }
    }

    private void SpawnRandomPrefabFromPortal()
    {
        if (Prefabs.Count > 0)
        {
            int index = Random.Range(0, Prefabs.Count);
            Debug.Log($"Spawning prefab at index: {index}"); // Debug statement
            Instantiate(Prefabs[index], transform.position, transform.rotation);
        }
    }

    private void DespawnPortal()
    {
        isSpawning = false;
        Destroy(gameObject);
    }
}
