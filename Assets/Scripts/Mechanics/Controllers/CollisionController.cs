using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class CollisionController : MonoBehaviour
    {
        public Health health { get; private set; }

        [SerializeField]
        private float duplicateCollisionAvoidanceTimer = 0f;

        public float DuplicateCollisionAvoidanceTimer
        {
            get => duplicateCollisionAvoidanceTimer;
            set => duplicateCollisionAvoidanceTimer = value;
        }

        private HashSet<GameObject> recentCollisions = new HashSet<GameObject>();

        private void Start()
        {
            health = GetComponent<Health>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody parentRigidbody = other.GetComponentInParent<Rigidbody>();
            if (parentRigidbody != null)
            {
                GameObject collisionGameObject = parentRigidbody.gameObject;

                if (!recentCollisions.Contains(collisionGameObject))
                {
                    recentCollisions.Add(collisionGameObject);
                    StartCoroutine(RemoveFromRecentCollisions(collisionGameObject));

                    Debug.Log($"Collision detected on {gameObject.name} ({LayerMask.LayerToName(gameObject.layer)} Layer) from {other.gameObject.name} on ({LayerMask.LayerToName(other.gameObject.layer)} Layer)");

                    if (other.gameObject.layer == LayerMask.NameToLayer(Layers.Battle.ToString()))
                    {
                        if (TryGetComponentInHierarchy(other.transform, out ProjectileController projectile))
                        {
                            Destroy(other.gameObject);
                            health.Decrement(projectile.Damage);
                        }
                        else if (TryGetComponentInHierarchy(other.transform, out MeleeController melee))
                        {
                            health.Decrement(melee.Damage);
                        }
                    }
                }
            }
        }

        private bool TryGetComponentInHierarchy<T>(Transform transform, out T component) where T : Component
        {
            component = transform.GetComponent<T>();
            while (component == null && transform.parent != null)
            {
                transform = transform.parent;
                component = transform.GetComponent<T>();
            }
            return component != null;
        }

        private IEnumerator RemoveFromRecentCollisions(GameObject obj)
        {
            if (duplicateCollisionAvoidanceTimer > 0)
            {
                yield return new WaitForSeconds(duplicateCollisionAvoidanceTimer);
            }
            recentCollisions.Remove(obj);
        }
    }
}
