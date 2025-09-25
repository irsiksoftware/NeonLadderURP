using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// Implements Metroid-style 180-degree analog aiming using right stick input.
    /// Provides smooth analog aiming within a forward-facing arc, with support for
    /// both controller and mouse input fallback.
    /// </summary>
    public class AnalogAim180 : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform to rotate for aiming (should be at weapon/arm position)")]
        public Transform aimPivot;

        [Tooltip("Optional: Animator to drive upper-body poses")]
        public Animator animator;

        [Tooltip("SpriteRenderer for facing direction detection")]
        public SpriteRenderer spriteRenderer;

        [Header("Input Configuration")]
        [Tooltip("Right stick input for analog aiming")]
        public InputActionProperty aimStick;

        [Header("Aiming Parameters")]
        [Range(0f, 1f)]
        [Tooltip("Deadzone threshold for stick input")]
        public float deadzone = 0.2f;

        [Tooltip("How fast the aim tracks the stick input (degrees/second)")]
        public float rotateSpeedDegPerSec = 720f;

        [Tooltip("Hysteresis to prevent facing direction chatter")]
        public float hysteresisDeg = 10f;

        [Header("Angle Constraints")]
        [Tooltip("Minimum local angle in degrees (-90 = down-forward)")]
        public float minLocalDeg = -90f;

        [Tooltip("Maximum local angle in degrees (+90 = up-forward)")]
        public float maxLocalDeg = +90f;

        [Header("Integration")]
        [Tooltip("Should analog aim override mouse when active?")]
        public bool overrideMouseWhenActive = true;

        // Private state
        private float currentLocalDeg;
        private int facingSign = 1; // +1 = facing right, -1 = facing left
        private bool isAnalogActive;

        // Public properties for integration
        public bool IsAnalogActive => isAnalogActive;
        public Vector2 CurrentAimDirection => aimPivot ? aimPivot.right : Vector2.right;
        public float CurrentAimAngleNormalized => Mathf.InverseLerp(minLocalDeg, maxLocalDeg, currentLocalDeg) * 2f - 1f;
        public float CurrentStickMagnitude { get; private set; }

        void OnEnable()
        {
            aimStick.action?.Enable();
        }

        void OnDisable()
        {
            aimStick.action?.Disable();
        }

        void Update()
        {
            ProcessAnalogAiming();
        }

        private void ProcessAnalogAiming()
        {
            if (!aimPivot || aimStick.action == null) return;

            // Read analog stick input
            Vector2 stickInput = aimStick.action.ReadValue<Vector2>();
            float stickMagnitude = stickInput.magnitude;
            CurrentStickMagnitude = stickMagnitude;

            // Check if analog input is active
            isAnalogActive = stickMagnitude >= deadzone;

            if (!isAnalogActive)
            {
                // Optional: Ease back to neutral or keep last angle
                SetAnimatorValues(0f, 0f);
                return;
            }

            // Update facing direction
            UpdateFacingDirection(stickInput);

            // Convert stick to local angle relative to facing direction
            float targetLocalDeg = CalculateTargetAngle(stickInput);

            // Smooth rotation toward target
            currentLocalDeg = Mathf.MoveTowards(
                currentLocalDeg,
                targetLocalDeg,
                rotateSpeedDegPerSec * Time.deltaTime
            );

            // Apply rotation to aim pivot
            ApplyAimRotation();

            // Update animator if present
            float normalizedAngle = Mathf.InverseLerp(minLocalDeg, maxLocalDeg, currentLocalDeg) * 2f - 1f;
            float normalizedMagnitude = Mathf.InverseLerp(deadzone, 1f, stickMagnitude);
            SetAnimatorValues(normalizedAngle, normalizedMagnitude);
        }

        private void UpdateFacingDirection(Vector2 stickInput)
        {
            // Determine facing based on sprite flip or stick X with hysteresis
            if (spriteRenderer)
            {
                int currentFacing = spriteRenderer.flipX ? -1 : 1;

                // Apply hysteresis to prevent direction chatter
                float hysteresisThreshold = Mathf.Sin(hysteresisDeg * Mathf.Deg2Rad);

                if (currentFacing > 0 && stickInput.x < -hysteresisThreshold)
                {
                    facingSign = -1;
                    spriteRenderer.flipX = true;
                }
                else if (currentFacing < 0 && stickInput.x > hysteresisThreshold)
                {
                    facingSign = 1;
                    spriteRenderer.flipX = false;
                }
                else
                {
                    facingSign = currentFacing;
                }
            }
        }

        private float CalculateTargetAngle(Vector2 stickInput)
        {
            // Convert stick input to local angle
            // Use absolute value of X to fold the input into forward-facing arc
            float localRad = Mathf.Atan2(stickInput.y, Mathf.Abs(stickInput.x));
            float targetLocalDeg = localRad * Mathf.Rad2Deg;

            // Clamp to valid range
            return Mathf.Clamp(targetLocalDeg, minLocalDeg, maxLocalDeg);
        }

        private void ApplyAimRotation()
        {
            // Calculate world rotation
            float baseDeg = (facingSign > 0) ? 0f : 180f; // Forward direction
            float worldZDeg = baseDeg + currentLocalDeg;

            aimPivot.rotation = Quaternion.Euler(0f, 0f, worldZDeg);
        }

        private void SetAnimatorValues(float normalizedAngle, float normalizedMagnitude)
        {
            if (!animator) return;

            animator.SetFloat("AimAngle", normalizedAngle);     // -1 to +1
            animator.SetFloat("AimStrength", normalizedMagnitude); // 0 to 1
        }

        /// <summary>
        /// Get the current aim direction in world space for weapon systems
        /// </summary>
        public Vector2 GetAimDirection2D()
        {
            return aimPivot ? aimPivot.right : Vector2.right;
        }

        /// <summary>
        /// Get a world position for targeting at specified distance
        /// </summary>
        public Vector3 GetAimTargetPosition(float distance = 10f)
        {
            if (!aimPivot) return transform.position + Vector3.right * distance;

            Vector2 aimDir = GetAimDirection2D();
            return aimPivot.position + (Vector3)(aimDir * distance);
        }

        /// <summary>
        /// Force set the aim angle (useful for external systems)
        /// </summary>
        public void SetAimAngle(float localDegrees)
        {
            currentLocalDeg = Mathf.Clamp(localDegrees, minLocalDeg, maxLocalDeg);
            ApplyAimRotation();
        }

        #region Editor Helpers
        void OnDrawGizmosSelected()
        {
            if (!aimPivot) return;

            // Draw aim direction
            Gizmos.color = isAnalogActive ? Color.red : Color.yellow;
            Vector3 aimDir = GetAimDirection2D();
            Gizmos.DrawRay(aimPivot.position, aimDir * 3f);

            // Draw valid aim arc
            Gizmos.color = Color.blue;
            float baseDeg = (facingSign > 0) ? 0f : 180f;

            for (float angle = minLocalDeg; angle <= maxLocalDeg; angle += 10f)
            {
                float worldAngle = (baseDeg + angle) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(worldAngle), Mathf.Sin(worldAngle), 0f);
                Gizmos.DrawRay(aimPivot.position, dir * 2f);
            }
        }
        #endregion
    }
}