using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    public class EnemyDeath : BaseGameEvent<EnemyDeath>
    {
        float DeathAnimationDuration = 3.5f;
        public Monster enemyController;
        public GameObject enemyGameObject;
        public Animator enemyAnimator;

        public override void Execute()
        {
            enemyGameObject = GameObject.Find("Karcinomorph_Tint1");
            enemyController = enemyGameObject.GetComponentInChildren<Monster>();
            enemyAnimator = enemyGameObject.GetComponent<Animator>();

            enemyAnimator.SetInteger("animation", 4);
            enemyController.StartCoroutine(HandleDeathAnimation(enemyGameObject, enemyAnimator));
        }

        private IEnumerator HandleDeathAnimation(GameObject enemyGameObject, Animator enemyAnimator)
        {
            yield return new WaitForSeconds(DeathAnimationDuration);
            enemyAnimator.enabled = false;
            enemyGameObject.SetActive(false);
        }
    }
}
