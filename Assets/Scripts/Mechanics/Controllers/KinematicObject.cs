using NeonLadder.Core;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class KinematicObject : MonoBehaviour
    {
        protected Player player;
        public float minGroundNormalY = .65f;
        public float gravityModifier = 1f;
        public Vector3 velocity;
        public LayerMask layerMask;
        public bool IsGrounded { get; private set; }
        protected Vector3 targetVelocity;
        protected Vector3 groundNormal;
        //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/Component.deprecated.cs
        protected new Rigidbody rigidbody;
        protected RaycastHit[] hitBuffer = new RaycastHit[16];
        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;

        public void Bounce(float value)
        {
            velocity.y = value;
        }

        public void Teleport(Vector3 position)
        {
            rigidbody.position = position;
            velocity = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
        }

        protected virtual void OnEnable()
        {
            rigidbody = GetComponentInParent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        protected virtual void OnDisable()
        {
            rigidbody.isKinematic = false;
        }

        protected virtual void Start()
        {
            player = Simulation.GetModel<PlatformerModel>().Player;
        }

        protected virtual void Update()
        {
            targetVelocity = Vector3.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {
            targetVelocity = Vector3.zero;
        }

        protected virtual void FixedUpdate()
        {
            if (rigidbody.useGravity)
            {
                ApplyGravity();
            }

            UpdateHorizontalVelocity();

            Vector3 deltaPosition = velocity * Time.deltaTime;

            // Adjust moveAlongGround calculation based on whether gravity is applied
            Vector3 moveAlongGround = rigidbody.useGravity ? new Vector3(groundNormal.y, -groundNormal.x, 0) : Vector3.right;

            Vector3 horizontalMove = moveAlongGround * deltaPosition.x;
            PerformMovement(horizontalMove, false);

            Vector3 verticalMove = Vector3.up * deltaPosition.y;
            PerformMovement(verticalMove, true);
        }

        void ApplyGravity()
        {
            velocity += gravityModifier * Physics.gravity * Time.deltaTime;
        }

        void UpdateHorizontalVelocity()
        {
            velocity.x = targetVelocity.x;
            velocity.z = 0;
        }

        void PerformMovement(Vector3 move, bool isVerticalMovement)
        {
            float distance = move.magnitude;
            if (distance > minMoveDistance)
            {
                Vector3 rayOrigin = transform.position;
                Vector3 rayDirection = move.normalized;

                int hitCount = Physics.RaycastNonAlloc(rayOrigin, rayDirection, hitBuffer, distance + shellRadius, layerMask);
                for (int i = 0; i < hitCount; i++)
                {
                    Vector3 currentNormal = hitBuffer[i].normal;
                    if (currentNormal.y > minGroundNormalY)
                    {
                        IsGrounded = true;
                        if (isVerticalMovement)
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    if (IsGrounded)
                    {
                        AdjustVelocityForGroundCollision(currentNormal);
                    }
                    else if (rigidbody.useGravity)
                    {
                        AdjustVelocityForAirCollision();
                    }

                    float modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = Mathf.Min(modifiedDistance, distance);
                }
            }

            rigidbody.position += move.normalized * distance;
        }

        void AdjustVelocityForGroundCollision(Vector3 normal)
        {
            float projection = Vector3.Dot(velocity, normal);
            if (projection < 0)
            {
                velocity -= projection * normal;
            }
        }

        void AdjustVelocityForAirCollision()
        {
            velocity.y = Mathf.Min(velocity.y, 0);
        }
    }
}
