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
            enemyAnimator = enemy.GetComponentInParent<Animator>();
            enemyAnimator.SetInteger("animation", enemy.deathAnimation);
            enemy.StartCoroutine(HandleDeathAnimation());
        }

        private IEnumerator HandleDeathAnimation()
        {
            yield return new WaitForSeconds(enemy.deathAnimationDuration);
            enemyAnimator.enabled = false;
            enemy.DropLoot(); // Drop items upon death
            enemy.transform.parent.gameObject.SetActive(false);
        }
    }
}
