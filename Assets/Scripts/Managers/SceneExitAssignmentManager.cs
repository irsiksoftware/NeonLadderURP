using NeonLadder.Common;
using System;
using System.Linq;
using UnityEngine;

public class SceneExitAssignmentManager : MonoBehaviour
{
    void Start()
    {
        enabled = false;
        GameObject[] sceneChangeObjects = GameObject.FindGameObjectsWithTag("SceneChange");

        // Shuffle the array
        sceneChangeObjects = sceneChangeObjects.OrderBy(x => Guid.NewGuid()).ToArray();

        bool hasBoss = false;
        bool hasMinorEnemy = false;

        // Check if we have exactly three scene change objects
        for (int i = 0; i < sceneChangeObjects.Length; i++)
        {
            // Assign one boss and one minor enemy scene
            if (!hasBoss)
            {
                AssignScene(sceneChangeObjects[i], SelectRandomBossScene(), "boss");
                hasBoss = true;
                continue;
            }
            if (!hasMinorEnemy)
            {
                AssignScene(sceneChangeObjects[i], SelectRandomMinorEnemyScene(), "minor-enemy");
                hasMinorEnemy = true;
                continue;
            }
            AssignScene(sceneChangeObjects[i], SelectRandomScene(), "random");
        }

    }


    private void AssignScene(GameObject sceneChangeObject, string sceneName, string sceneType)
    {
        var sceneChanger = sceneChangeObject.GetComponent<SceneChangeController>();
        if (sceneChanger != null)
        {
            sceneChanger.SceneName = sceneName;
        }
        else
        {
            Debug.LogWarning("SceneChangeManager component not found on object with 'SceneChange' tag.");
        }
    }


    private string SelectRandomBossScene()
    {
        var remainingBosses = Constants.Bosses.Except(Constants.DefeatedBosses).ToList();
        return remainingBosses.ToList()[UnityEngine.Random.Range(0, remainingBosses.Count)];
    }

    private string SelectRandomShopScene()
    {
        return Constants.ShopLevels[UnityEngine.Random.Range(0, Constants.ShopLevels.Count)];
    }

    private string SelectRandomMinorEnemyScene()
    {
        return Constants.MinorEnemyLevels[UnityEngine.Random.Range(0, Constants.MinorEnemyLevels.Count)];
    }
    private string SelectRandomMajorEnemyScene()
    {
        return Constants.MajorEnemyLevels[UnityEngine.Random.Range(0, Constants.MajorEnemyLevels.Count)];
    }

    private string SelectRandomScene()
    {
        var scene = UnityEngine.Random.Range(0, 2);
        if (scene == 0)
        {
            return SelectRandomMajorEnemyScene();
        }
        else
        {
            return SelectRandomShopScene();
        }
    }
}