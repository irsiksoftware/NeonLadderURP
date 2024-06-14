using NeonLadder.Mechanics.Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterGroupActivationManager : MonoBehaviour
{
    public List<MonsterGroup> MonsterGroups { get; set; } = new List<MonsterGroup>();
    // Start is called before the first frame update
    void Start()
    {
        var monsterGroups = GameObject.FindGameObjectsWithTag("MonsterGroup");
        foreach (var monsterGroup in monsterGroups)
        {
            var monsters = monsterGroup.GetComponentsInChildren<Health>().ToList();
            MonsterGroups.Add(new MonsterGroup
            {
                GameObjectName = monsterGroup.name,
                Monsters = monsters
            });
        }

        //leave one group active, deactivate all others
        foreach (var monsterGroup in MonsterGroups)
        {
            if (monsterGroup.GameObjectName == "Group1")
            {
                foreach (var monster in monsterGroup.Monsters)
                {
                    monster.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (var monster in monsterGroup.Monsters)
                {
                    monster.gameObject.SetActive(false);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        //if all monsters in a group are dead, deactivate the group, and randomly activate another group until all groups are dead
        foreach (var monsterGroup in MonsterGroups)
        {
            if (monsterGroup.Monsters.All(monster => !monster.IsAlive))
            {
                var randomMonsterGroup = MonsterGroups.FirstOrDefault(mg => mg.Monsters.Any(m => m.IsAlive));
                if (randomMonsterGroup?.Monsters != null)
                {
                    foreach (var monster in randomMonsterGroup.Monsters)
                    {
                        if (monster.IsAlive)
                        {
                            monster.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
