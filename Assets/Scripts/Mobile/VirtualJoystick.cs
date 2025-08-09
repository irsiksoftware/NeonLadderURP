using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonLadder.Mobile
{
    /// <summary>
    /// Virtual joystick implementation for mobile controls.
    /// MUST BE TESTED ON ACTUAL DEVICES for feel and responsiveness.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        
        [Header("Settings")]
        [SerializeField] public float deadZone = 0.1f;
        [SerializeField] private float handleRange = 1f;
        [SerializeField] private bool fixedPosition = true;
        [SerializeField] private bool snapToCenter = true;
        
        [Header("Visual")]
        [SerializeField] private bool hideWhenInactive = false;
        [SerializeField] private float inactiveAlpha = 0.3f;
        [SerializeField] private float activeAlpha = 0.6f;
        
        // Events
        public event Action<Vector2> OnValueChanged;
        
        // State
        private Vector2 input = Vector2.zero;
        private bool isActive = false;
        private bool isCustomizing = false;
        private Canvas canvas;
        private Camera cam;
        
        // For dynamic positioning
        private Vector2 joystickStartPosition;
        
        public Vector2 Direction => input;
        public float Horizontal => input.x;
        public float Vertical => input.y;
        
        private void Start()
        {
            // Get references if not set
            if (background == null)
                background = GetComponent<RectTransform>();
            
            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0).GetComponent<RectTransform>();
            
            canvas = GetComponentInParent<Canvas>();
            
            if (canvas != null)
            {
                cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            }
            
            // Set initial visual state
            UpdateVisualState(false);
            
            // Store center position
            joystickStartPosition = background.anchoredPosition;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isCustomizing) return;
            
            isActive = true;
            UpdateVisualState(true);
            
            // For floating joystick, move to touch position
            if (!fixedPosition)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    eventData.position,
                    cam,
                    out position
                );
                
                background.anchoredPosition = position;
            }
            
            OnDrag(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isCustomizing) return;
            
            isActive = false;
            UpdateVisualState(false);
            
            input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            OnValueChanged?.Invoke(input);
            
            // Return to original position if floating
            if (!fixedPosition && snapToCenter)
            {
                background.anchoredPosition = joystickStartPosition;
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (isCustomizing)
            {
                // In customization mode, move the entire joystick
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    eventData.position,
                    cam,
                    out position
                );
                
                background.anchoredPosition = position;
                joystickStartPosition = position; // Update stored position
                return;
            }
            
            if (!isActive) return;
            
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                cam,
                out position
            );
            
            // Calculate input
            position = Vector2.ClampMagnitude(position, background.sizeDelta.x / 2f * handleRange);
            handle.anchoredPosition = position;
            
            float handleRangePixels = background.sizeDelta.x / 2f * handleRange;
            input = position / handleRangePixels;
            
            // Apply dead zone
            if (input.magnitude < deadZone)
            {
                input = Vector2.zero;
            }
            else
            {
                // Remap to remove dead zone from input
                float magnitude = input.magnitude;
                input = input.normalized * ((magnitude - deadZone) / (1f - deadZone));
            }
            
            // Clamp to unit circle
            input = Vector2.ClampMagnitude(input, 1f);
            
            OnValueChanged?.Invoke(input);
        }
        
        private void UpdateVisualState(bool active)
        {
            if (hideWhenInactive && !active)
            {
                gameObject.SetActive(false);
                return;
            }
            
            // Update alpha
            float targetAlpha = active ? activeAlpha : inactiveAlpha;
            
            Image bgImage = background?.GetComponent<Image>();
            if (bgImage != null)
            {
                Color c = bgImage.color;
                c.a = targetAlpha;
                bgImage.color = c;
            }
            
            Image handleImage = handle?.GetComponent<Image>();
            if (handleImage != null)
            {
                Color c = handleImage.color;
                c.a = targetAlpha;
                handleImage.color = c;
            }
        }
        
        public void EnableCustomization(bool enable)
        {
            isCustomizing = enable;
            
            if (enable)
            {
                // Visual feedback for customization mode
                Image bgImage = background?.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = new Color(1, 1, 0, 0.5f); // Yellow tint
                }
            }
            else
            {
                // Restore normal appearance
                UpdateVisualState(false);
            }
        }
        
        public void ResetPosition()
        {
            background.anchoredPosition = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            input = Vector2.zero;
            OnValueChanged?.Invoke(input);
        }
        
        // For testing in editor
        private void OnDrawGizmosSelected()
        {
            if (background == null) return;
            
            // Draw dead zone
            Gizmos.color = Color.red;
            Vector3 center = transform.position;
            float radius = (background.sizeDelta.x / 2f) * deadZone * transform.lossyScale.x;
            
            for (int i = 0; i < 32; i++)
            {
                float angle1 = (i / 32f) * Mathf.PI * 2f;
                float angle2 = ((i + 1) / 32f) * Mathf.PI * 2f;
                
                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                
                Gizmos.DrawLine(p1, p2);
            }
            
            // Draw active range
            Gizmos.color = Color.green;
            radius = (background.sizeDelta.x / 2f) * handleRange * transform.lossyScale.x;
            
            for (int i = 0; i < 32; i++)
            {
                float angle1 = (i / 32f) * Mathf.PI * 2f;
                float angle2 = ((i + 1) / 32f) * Mathf.PI * 2f;
                
                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}