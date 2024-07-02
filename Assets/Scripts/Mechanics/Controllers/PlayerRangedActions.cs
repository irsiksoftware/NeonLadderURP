using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class PlayeRangedActions : MonoBehaviour
    {
        public float raycastDistance = 100f;
        public Transform spawnPoint;
        public float projectileSpeed = 10f; // Speed of the projectile
        private int currentFX = 0;
        public List<GameObject> prefabs = new List<GameObject>();
        public Player player;

        public void Shoot(Vector3 targetPoint)
        {
            GameObject instance = Instantiate(prefabs[currentFX], spawnPoint.position, spawnPoint.rotation);
            Vector3 direction = (targetPoint - spawnPoint.position).normalized;
            instance.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
            instance.GetComponent<ProjectileController>().SpawnSubFX(instance.GetComponent<ProjectileController>().muzzle, spawnPoint);
        }

        private void Awake()
        {
            player = GetComponentInParent<Player>();
        }

        void Update()
        {
            if (!player.Actions.isUsingMelee)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 mousePos = Input.mousePosition;

                    // Use the z-coordinate of the spawn point to convert mouse screen position to world position
                    Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.WorldToScreenPoint(spawnPoint.position).z));

                    // Ensure the mouse position is in the same plane as the spawn point
                    mouseWorldPosition.z = spawnPoint.position.z;

                    // Log mouse positions for debugging
                    Debug.Log($"Mouse Screen Position: {mousePos}, Mouse World Position: {mouseWorldPosition}");

                    // Perform the 2D raycast from the mouse world position
                    RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, raycastDistance);
                    Vector3 targetPoint = hit.collider != null ? new Vector3(hit.point.x, hit.point.y, spawnPoint.position.z) : mouseWorldPosition;

                    // Draw the raycast line always
                    Debug.DrawLine(spawnPoint.position, targetPoint, Color.red);

                    if (Input.GetMouseButtonDown(0))
                    {
                        Debug.Log($"Spawn Point Position: {spawnPoint.position}, Cursor Position: {targetPoint}, MousePos: {mousePos}");
                        Shoot(targetPoint);
                    }
                }
            }
        }
    }
}
