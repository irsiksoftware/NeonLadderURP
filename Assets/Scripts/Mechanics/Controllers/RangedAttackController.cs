using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class RangedAttackController : MonoBehaviour
    {
        [Header("Projectile Settings")]
        public float raycastDistance = 100f;
        public Transform spawnPoint;
        private int currentFX = 0;
        public List<GameObject> prefabs = new List<GameObject>();
        public Player player;
        public int Damage = 10;
        private float projectileLifetime = 2f;

        [Header("Analog Aiming Integration")]
        [Tooltip("Optional analog aiming component for 180-degree controller support")]
        public AnalogAim180 analogAim;

        [Tooltip("Should analog aiming override mouse when active?")]
        public bool analogOverridesMouse = true;

        [Header("Mouse Aiming Fallback")]
        [Tooltip("Use mouse aiming when analog is not active")]
        public bool enableMouseFallback = true;

        public void Shoot(Vector3 targetPoint)
        {
            GameObject instance = Instantiate(prefabs[currentFX], spawnPoint.position, Quaternion.identity);
            Destroy(instance, projectileLifetime);
            ProjectileController projectileController = instance.GetComponent<ProjectileController>();

            if (projectileController != null)
            {
                projectileController.Damage = Damage;
                Vector3 direction = (targetPoint - spawnPoint.position).normalized;
                direction.z = 0;
                projectileController.SetDirection(direction);
            }

            projectileController.SpawnSubFX(projectileController.muzzle, spawnPoint, projectileLifetime);
        }

        private void Awake()
        {
            player = GetComponent<Player>();

            // Auto-find analog aiming component if not assigned
            if (analogAim == null)
            {
                analogAim = GetComponent<AnalogAim180>();
            }
        }

        void Update()
        {
            if (!player.IsUsingMelee)
            {
                HandleAiming();
            }
        }

        private void HandleAiming()
        {
            Vector3 targetPoint;
            bool shouldUseAnalog = analogAim != null && analogAim.IsAnalogActive && analogOverridesMouse;

            if (shouldUseAnalog)
            {
                // Use analog aiming
                targetPoint = GetAnalogTargetPoint();
                Debug.DrawLine(spawnPoint.position, targetPoint, Color.cyan); // Cyan for analog
            }
            else if (enableMouseFallback)
            {
                // Fall back to mouse aiming
                targetPoint = GetMouseTargetPoint();
                if (targetPoint != Vector3.zero)
                {
                    Debug.DrawLine(spawnPoint.position, targetPoint, Color.red); // Red for mouse
                }
                else
                {
                    return; // No valid mouse position
                }
            }
            else
            {
                return; // No aiming input available
            }

            // Handle shooting input (works for both analog and mouse)
            if (Input.GetMouseButtonDown(0)) // TODO: Replace with Input Action for controller support
            {
                Shoot(targetPoint);
            }
        }

        private Vector3 GetAnalogTargetPoint()
        {
            // Get target position from analog aiming system
            Vector3 targetPoint = analogAim.GetAimTargetPosition(raycastDistance);

            // Optional: Raycast to find actual collision point
            Vector2 aimDirection = analogAim.GetAimDirection2D();
            RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, aimDirection, raycastDistance);

            if (hit.collider != null)
            {
                return new Vector3(hit.point.x, hit.point.y, spawnPoint.position.z);
            }

            return targetPoint;
        }

        private Vector3 GetMouseTargetPoint()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector3 mousePos = Input.mousePosition;
            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.WorldToScreenPoint(spawnPoint.position).z));
            mouseWorldPosition.z = spawnPoint.position.z;

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, raycastDistance);
            return hit.collider != null ? new Vector3(hit.point.x, hit.point.y, spawnPoint.position.z) : mouseWorldPosition;
        }
    }
}
