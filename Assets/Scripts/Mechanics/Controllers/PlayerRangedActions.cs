using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using UnityEngine;

public class PlayeRangedActions : MonoBehaviour
{
    public Camera cam;
    public float raycastDistance;
    public Transform spawnPoint;
    private Ray mouseR;
    private Vector3 dir;
    private Quaternion rotation;
    private int currentFX = 0;
    public List<GameObject> prefabs = new List<GameObject>();
    public Player player;

    public void Shoot()
    {
        GameObject instance = Instantiate(prefabs[currentFX], spawnPoint.position, spawnPoint.rotation);
        instance.GetComponent<ProjectileController>().SpawnSubFX(instance.GetComponent<ProjectileController>().muzzle, spawnPoint);
    }
    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    void Update()
    {
        if (cam != null)
        {
            RaycastHit hit;
            Vector3 mousePos = Input.mousePosition;
            mouseR = cam.ScreenPointToRay(mousePos);
            bool hitSomething = Physics.Raycast(mouseR.origin, mouseR.direction, out hit, raycastDistance);
            Vector3 targetPoint = hitSomething ? hit.point : mouseR.GetPoint(raycastDistance);

            RotateToMouseDirection(gameObject, targetPoint);

            // Draw the raycast line always
            Debug.DrawLine(spawnPoint.position, targetPoint, Color.red);

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"Spawn Point Position: {spawnPoint.position}, Cursor Position: {targetPoint}");
                if (!player.Actions.isUsingMelee)
                {
                    Shoot();
                }
            }
        }
    }

    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        dir = destination - obj.transform.position;
        rotation = Quaternion.LookRotation(dir);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }
}
