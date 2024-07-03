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

        public override void Execute()
        {
            enemy.GetComponentInParent<Rigidbody>().useGravity = true;
            //change enemy layer to FX on death do bullets pass through.
            enemy.gameObject.layer = LayerMask.NameToLayer(Layers.Dead.ToString());
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
