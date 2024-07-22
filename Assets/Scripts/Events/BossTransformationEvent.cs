using NeonLadder.Common;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Utilities;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    public class BossTransformationEvent : BaseGameEvent<BossTransformationEvent>
    {
        public Boss boss;

        public override void Execute()
        {
            if (boss.transformation != null)
            {
                boss.transformation.transform.position = boss.transform.position;
                var transformedBoss = boss.transformation.GetComponentInChildren<Boss>();
                if (transformedBoss != null)
                {
                    transformedBoss.Orient();
                    transformedBoss.ShouldEngagePlayer = false;
                    CoroutineRunner.RunCoroutine(ScaleOverTime(boss.transformation, Constants.TransformationDurationInSeconds, transformedBoss));
                }
                boss.transformation.SetActive(true);
            }
            else
            {
                Debug.LogError($"No transformation GameObject found for boss '{boss.transform.parent.name}'.");
            }
        }

        private IEnumerator ScaleOverTime(GameObject target, float duration, Boss transformedBoss)
        {
            Vector3 initialScale = target.transform.localScale;
            Vector3 targetScale = Vector3.one;
            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float elapsedTime = Time.time - startTime;
                float t = elapsedTime / duration;
                target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                yield return null;
            }

            target.transform.localScale = targetScale;
            transformedBoss.ShouldEngagePlayer = true;
        }
    }
}
