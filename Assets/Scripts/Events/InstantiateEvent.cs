using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for coordinated GameObject instantiation
    /// Replaces direct Instantiate() calls throughout the codebase
    /// </summary>
    public class InstantiateEvent : Simulation.Event
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public Transform parent;
        public float destroyAfter = -1f; // -1 means don't auto-destroy
        public string instantiationType;

        public override bool Precondition()
        {
            return prefab != null;
        }

        public override void Execute()
        {
            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
                
                // Schedule destruction if specified
                if (destroyAfter > 0f)
                {
                    var destroyEvent = Simulation.Schedule<DestroyEvent>(destroyAfter);
                    destroyEvent.gameObject = instance;
                }
                
                // Trigger any post-instantiation effects
                TriggerPostInstantiationEffects(instance);
            }
        }

        private void TriggerPostInstantiationEffects(GameObject instance)
        {
            switch (instantiationType)
            {
                case "Projectile":
                    // Schedule projectile-specific setup
                    var projectile = instance.GetComponent<ProjectileController>();
                    if (projectile != null)
                    {
                        // Could schedule projectile movement events here
                    }
                    break;
                case "Collectible":
                    // Schedule collectible-specific setup
                    break;
                case "VFX":
                    // Schedule particle system activation
                    var particleSystem = instance.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        particleSystem.Play(true);
                    }
                    break;
            }
        }

        internal override void Cleanup()
        {
            prefab = null;
            parent = null;
        }
    }

    /// <summary>
    /// Event for destroying GameObjects after a delay
    /// </summary>
    public class DestroyEvent : Simulation.Event
    {
        public GameObject gameObject;

        public override bool Precondition()
        {
            return gameObject != null;
        }

        public override void Execute()
        {
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }
        }

        internal override void Cleanup()
        {
            gameObject = null;
        }
    }
}