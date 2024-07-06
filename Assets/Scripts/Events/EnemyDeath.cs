using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Events
{
    public class EnemyDeath : BaseGameEvent<EnemyDeath>
    {
        public Enemy enemy;
        public Animator enemyAnimator;
        private GameObject thisActorParent;

        public override void Execute()
        {
            thisActorParent = enemy.transform.parent.gameObject;
            thisActorParent.GetComponent<Rigidbody>().useGravity = true;
            thisActorParent.GetComponentInChildren<CollisionController>().enabled = false;

            //change enemy layer to FX on death so bullets pass through.
            enemy.transform.parent.gameObject.layer = LayerMask.NameToLayer(Layers.Dead.ToString());

            //have to change the layer to avoid stopping player from walking over the enemy after they've died.
            var attackComponents = enemy.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                    .Where(c => c.gameObject != enemy.transform.parent.gameObject);

            foreach (var attackComponent in attackComponents)
            {
                attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Dead));
            }

            GameObject.FindGameObjectWithTag(Tags.Managers.ToString())
                      .GetComponentInChildren<LootDropManager>()
                      .DropLoot(model.Player, enemy);
        }
    }
}
