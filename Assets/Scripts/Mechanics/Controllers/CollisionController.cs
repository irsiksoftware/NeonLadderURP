using NeonLadder.Common;
using NeonLadder.Events;
using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Controllers
{
    public class CollisionController : MonoBehaviour
    {
        public Health health { get; private set; }
        private KinematicObject otherActor;
        private GameObject otherActorParent;
        
        private KinematicObject thisActor;
        private GameObject thisActorParent;

        // Auto-property with default value and validation logic
        [SerializeField]
        private float duplicateCollisionAvoidanceTimer = 0.6f;
        public virtual float DuplicateCollisionAvoidanceTimer
        {
            get => duplicateCollisionAvoidanceTimer;
            set => duplicateCollisionAvoidanceTimer = Mathf.Max(0.6f, value);
        }

        private HashSet<GameObject> recentCollisions = new HashSet<GameObject>();

        private void Awake()
        {
            //ugh
            DuplicateCollisionAvoidanceTimer = duplicateCollisionAvoidanceTimer;
     
            thisActorParent = GetComponentInParent<Rigidbody>()?.gameObject;
            if (thisActorParent == null)
            {
                Debug.Log($"{gameObject.name} does not appear to have a parent with a rigidbody.");
            }
            else
            {
                health = thisActorParent.GetComponent<Health>();
                thisActor = thisActorParent.GetComponentInChildren<KinematicObject>();
            }
        }

        private void OnEnable()
        {
            if (ManagerController.Instance == null)
            {
                Debug.Log("Managers prefab is missing, or it's instance is missing an implementaiton.");
            }
            else
            {
                ManagerController.Instance.eventManager.StartListening("OnTriggerEnter", thisActorParent, OnTriggerEnter);
            }
        }

        private void OnDisable()
        {
            //This stuff happens on Actor death.
            //if (ManagerController.Instance == null)
            //{
            //    Debug.Log("Managers prefab is missing, or it's instance is missing an implementaiton.");
            //}
            //else
            //{
            //    ManagerController.Instance.eventManager.StopListening("OnTriggerEnter", thisActorParent, OnTriggerEnter);
            //}
        }

        private void OnTriggerEnter(Collider other)
        {

            // If collider is type of terrain collider, schedule terrain collision event
            if (other is TerrainCollider && thisActor is Player)
            {
                Schedule<PlayerTerrainCollision>();
                return;
            }
            else if (other is TerrainCollider && ((thisActor is Enemy) || thisActor is Boss))
            {
                var coll = Schedule<EnemyTerrainCollision>();
                coll.enemy = thisActor as Enemy;

                return;
            }
            else if (thisActorParent?.layer == LayerMask.NameToLayer(Layers.Dead.ToString()) || other.gameObject.layer == LayerMask.NameToLayer(Layers.Dead.ToString()))
            {
                return;
            }
            

            // If we got this far, we know it's probably a legit attack collision.
            try
            {
                otherActorParent = other.GetComponentInParent<Rigidbody>().gameObject;
                otherActor = otherActorParent.GetComponentInChildren<KinematicObject>();
            }
            catch (Exception ex)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer(Layers.TransparentFX.ToString()) || other.tag == nameof(Tags.Untagged))
                {
                    //see Collectible class for this (for now, unsure if should move to here, or schedule event, etc), or something collided with an untagged object, likely a building.
                    return;
                }
                else
                {
                    Debug.LogError($"Error finding attacking actor: {ex.Message} \n {other.tag}");
                    return;
                }
            }

            if (otherActorParent.tag == nameof(Tags.PlayerProjectile) && thisActorParent.GetComponentInChildren<Player>() != null) //can't shoot myself.
            {
                return;
            }
            else if (thisActor is Enemy && otherActor is Enemy) //enemies can't hurt each other.
            {
                return;
            }
            else
            {
                if (!recentCollisions.Contains(otherActorParent))
                {
                    recentCollisions.Add(otherActorParent);
                    StartCoroutine(RemoveFromRecentCollisions(otherActorParent));

                    if (other.gameObject.layer == LayerMask.NameToLayer(Layers.Battle.ToString()))
                    {
                        if (otherActorParent.tag == nameof(Tags.PlayerProjectile))
                        {
                            health.Decrement(otherActorParent.GetComponent<ProjectileController>().Damage);
                            Destroy(otherActorParent);
                        }
                        else if (otherActor.IsUsingMelee)
                        {
                            //if (otherActor is Player)
                            //{
                            //    Debug.Log($"Other actor is player");
                            //}

                            //if (otherActor is Enemy)
                            //{
                            //    Debug.Log($"Other actor is enemy");
                            //}

                            //Debug.Log($"{thisActor.transform.parent.gameObject.name} took {otherActor.GetComponent<MeleeController>().Damage} damage from {otherActorParent.name} @ {Time.time}");
                            health.Decrement(otherActor.GetComponent<MeleeController>().Damage);
                            StartCoroutine(otherActor.PlayGetHitAnimation());
                        }
                        else
                        {
                            Debug.Log("Something went wrong");
                        }
                    }
                }
            }
        }


        private IEnumerator RemoveFromRecentCollisions(GameObject obj)
        {
            if (DuplicateCollisionAvoidanceTimer > 0)
            {
                yield return new WaitForSeconds(DuplicateCollisionAvoidanceTimer);
            }
            recentCollisions.Remove(obj);
        }
    }
}
