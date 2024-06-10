using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    public class FadeInCamera : BaseGameEvent<FadeInCamera>
    {
        public override void Execute()
        {
            FadeIn();
        }

        private IEnumerator FadeIn()
        {
            var virtualCamera = model.VirtualCamera;
            CanvasGroup canvasGroup = virtualCamera.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = virtualCamera.gameObject.AddComponent<CanvasGroup>();
            }

            float duration = 1.0f; // Duration of the fade
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / duration);
                yield return null;
            }

            canvasGroup.alpha = 0;
        }
    }
}
