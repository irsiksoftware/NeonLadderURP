using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Managers
{
    public class MonsterGroupActivationManager : MonoBehaviour
    {
        public List<MonsterGroup> MonsterGroups { get; set; } = new List<MonsterGroup>();
        void OnEnable()
        {
            var monsterGroups = GameObject.FindGameObjectsWithTag(Tags.MonsterGroup.ToString());
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

        void Awake()
        {
            enabled = false;
        }

        private void OnDisable()
        {
            MonsterGroups.Clear();
        }

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
                            if (monster != null && monster.IsAlive)
                            {
                                monster.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }
    }
}
