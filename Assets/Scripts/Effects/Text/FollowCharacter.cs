using NeonLadder.Mechanics.Controllers;
using UnityEngine;
using UnityEngine.UI; // Make sure to include this for the Image component

namespace NeonLadder.Effects.Text
{
    public class FollowCharacter : MonoBehaviour
    {
        public Transform target;
        public Vector3 offsetRight;
        public Vector3 offsetLeft;
        public Sprite rightBubbleSprite;
        public Sprite leftBubbleSprite;

        private RectTransform speechBubbleRect;
        private Image speechBubbleImage;
        private bool isFacingLeft;

        void Start()
        {
            speechBubbleRect = GetComponent<RectTransform>();
            speechBubbleImage = GetComponent<Image>();

            isFacingLeft = IsFacingLeft();
            speechBubbleImage.enabled = false;
        }

        void Update()
        {
            if (target != null)
            {
                isFacingLeft = IsFacingLeft();
                Vector3 worldOffset = isFacingLeft ? offsetLeft : offsetRight;
                Sprite currentSprite = isFacingLeft ? leftBubbleSprite : rightBubbleSprite;
                speechBubbleImage.sprite = currentSprite;
                Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(target.position + worldOffset);
                speechBubbleRect.position = screenPos;
                speechBubbleImage.enabled = true;
            }
        }

        private bool IsFacingLeft()
        {
            bool result;
            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            var bossController = target.GetComponent<Boss>();

            if (spriteRenderer != null)
            {
                result = spriteRenderer.flipX;
            }
            else if (bossController != null)
            {
                result = bossController.IsFacingLeft;
            }
            else
            {
                Debug.Log("No SpriteRenderer or BossController found on the target object. Defaulting to false.");
                result = true;
            }

            return result;
        }
    }
}