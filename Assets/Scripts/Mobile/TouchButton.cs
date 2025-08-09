using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonLadder.Mobile
{
    /// <summary>
    /// Touch button component for mobile controls.
    /// Handles press, release, and hold states with visual feedback.
    /// </summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Configuration")]
        public string buttonName = "Button";
        [SerializeField] private bool isToggle = false;
        [SerializeField] private bool allowHold = false;
        [SerializeField] private float holdThreshold = 0.5f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.gray;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private float pressedScale = 0.9f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pressSound;
        [SerializeField] private AudioClip releaseSound;
        
        // Events
        public event Action OnPressed;
        public event Action OnReleased;
        public event Action OnHold;
        public event Action<bool> OnToggled;
        
        // State
        private bool isPressed = false;
        private bool isToggled = false;
        private bool isHovering = false;
        private bool isCustomizing = false;
        private float pressTime = 0f;
        private bool holdTriggered = false;
        
        // Components
        private Image buttonImage;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Canvas canvas;
        private Camera cam;
        
        private void Awake()
        {
            buttonImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            originalScale = transform.localScale;
            
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            }
        }
        
        private void Update()
        {
            // Handle hold detection
            if (isPressed && allowHold && !holdTriggered)
            {
                pressTime += Time.deltaTime;
                
                if (pressTime >= holdThreshold)
                {
                    holdTriggered = true;
                    OnHold?.Invoke();
                    
                    // Visual feedback for hold
                    if (buttonImage != null)
                    {
                        buttonImage.color = Color.Lerp(pressedColor, Color.red, 0.5f);
                    }
                }
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isCustomizing)
            {
                // Start dragging in customization mode
                StartCoroutine(DragCoroutine(eventData));
                return;
            }
            
            isPressed = true;
            pressTime = 0f;
            holdTriggered = false;
            
            // Visual feedback
            if (buttonImage != null)
            {
                buttonImage.color = pressedColor;
            }
            
            transform.localScale = originalScale * pressedScale;
            
            // Audio feedback
            if (pressSound != null)
            {
                AudioSource.PlayClipAtPoint(pressSound, transform.position);
            }
            
            // Handle toggle
            if (isToggle)
            {
                isToggled = !isToggled;
                OnToggled?.Invoke(isToggled);
            }
            
            OnPressed?.Invoke();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isCustomizing) return;
            
            isPressed = false;
            pressTime = 0f;
            holdTriggered = false;
            
            // Visual feedback
            UpdateVisualState();
            transform.localScale = originalScale;
            
            // Audio feedback
            if (releaseSound != null)
            {
                AudioSource.PlayClipAtPoint(releaseSound, transform.position);
            }
            
            OnReleased?.Invoke();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isCustomizing) return;
            
            isHovering = true;
            UpdateVisualState();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isCustomizing) return;
            
            isHovering = false;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (buttonImage == null) return;
            
            if (isPressed)
            {
                buttonImage.color = pressedColor;
            }
            else if (isHovering)
            {
                buttonImage.color = hoverColor;
            }
            else if (isToggled)
            {
                buttonImage.color = Color.Lerp(normalColor, pressedColor, 0.5f);
            }
            else
            {
                buttonImage.color = normalColor;
            }
        }
        
        public void EnableCustomization(bool enable)
        {
            isCustomizing = enable;
            
            if (enable)
            {
                // Visual feedback for customization
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(1, 1, 0, 0.7f); // Yellow tint
                }
                
                // Add outline
                Outline outline = gameObject.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = gameObject.AddComponent<Outline>();
                }
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(2, 2);
                outline.enabled = true;
            }
            else
            {
                // Remove customization visuals
                UpdateVisualState();
                
                Outline outline = gameObject.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }
        
        private System.Collections.IEnumerator DragCoroutine(PointerEventData eventData)
        {
            while (isCustomizing && Input.GetMouseButton(0))
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    cam,
                    out position
                );
                
                rectTransform.anchoredPosition = position;
                
                yield return null;
            }
        }
        
        public void ResetPosition()
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        public void SetButtonSize(float size)
        {
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(size, size);
            }
        }
        
        public void SetOpacity(float opacity)
        {
            if (buttonImage != null)
            {
                Color c = buttonImage.color;
                c.a = opacity;
                buttonImage.color = c;
            }
        }
    }
}