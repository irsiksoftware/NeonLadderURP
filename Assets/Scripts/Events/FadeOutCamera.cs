using NeonLadder.Core;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    public class FadeOutCamera : BaseGameEvent<FadeOutCamera>
    {
        public override void Execute()
        {
            FadeOut();
        }

        private IEnumerator FadeOut()
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
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }
    }
}
