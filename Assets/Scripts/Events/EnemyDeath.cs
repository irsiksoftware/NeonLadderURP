using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
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
            enemy.gameObject.layer = LayerMask.NameToLayer(Layers.TransparentFX.ToString());
            GameObject.FindGameObjectWithTag(Tags.Managers.ToString())
                      .GetComponentInChildren<LootDropManager>()
                      .DropLoot(model.Player, enemy);
        }
    }
}
