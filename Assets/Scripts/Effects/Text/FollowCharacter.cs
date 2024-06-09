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

            speechBubbleImage.enabled = false;
        }

        void Update()
        {
            if (target != null)
            {
                Vector3 worldOffset = isFacingLeft ? offsetLeft : offsetRight;
                Sprite currentSprite = isFacingLeft ? leftBubbleSprite : rightBubbleSprite;
                speechBubbleImage.sprite = currentSprite;
                Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(target.position + worldOffset);
                speechBubbleRect.position = screenPos;
                speechBubbleImage.enabled = true;
            }
        }
    }
}