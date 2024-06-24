using NeonLadder.Mechanics.Controllers;
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
            Game.Instance.GetComponentInChildren<LootDropManager>().DropLoot(model.Player, enemy);
        }
    }
}
