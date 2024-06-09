using NeonLadder.Mechanics.Controllers;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    public class EnemyDeath : BaseGameEvent<EnemyDeath>
    {
        public Enemy enemy;
        public Animator enemyAnimator;

        public override void Execute()
        {
            //enemyAnimator = enemy.GetComponentInParent<Animator>();
            //enemyAnimator.SetInteger("animation", enemy.DeathAnimation);
            //enemy.StartCoroutine(HandleDeathAnimation());
            LootDropManager.DropLoot(model.Player, enemy); // Drop items upon death
        }

        //private IEnumerator HandleDeathAnimation()
        //{
        //    yield return new WaitForSeconds(enemy.DeathAnimationDuration);
        //    enemyAnimator.enabled = false;
        //    LootDropManager.DropLoot(enemy.RuntimeLootTable, enemy.transform, model.Player); // Drop items upon death
        //    enemy.transform.parent.gameObject.SetActive(false);
        //}
    }
}
