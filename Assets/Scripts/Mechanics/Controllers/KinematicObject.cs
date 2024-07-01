using NeonLadder.Core;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class KinematicObject : MonoBehaviour
    {
        public Player player { get; set; }
        public PlatformerModel model { get; private set; }
        public float minGroundNormalY = .75f;
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
            transform.position = position;
            velocity = Vector3.zero;
        }

        protected virtual void OnEnable()
        {

            switch (this)
            {
                case FlyingMinor:
                case Minor:
                case FlyingMajor:
                case Major:
                case Boss:
                case Enemy:
                    rigidbody = GetComponentInParent<Rigidbody>();
                    rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
                case Player:
                    rigidbody = GetComponent<Rigidbody>();
                    break;
                default:
                    break;
            }

            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
            }

            GuaranteeModelAndPlayer();
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Start()
        {
            GuaranteeModelAndPlayer();
        }

        protected virtual void Awake()
        {
            GuaranteeModelAndPlayer();
        }

        private void GuaranteeModelAndPlayer()
        {
            if (model == null)
            {
                model = Simulation.GetModel<PlatformerModel>();
            }
            if (player == null && model != null)
            {
                player = model.Player;
            }
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
            IsGrounded = false;

            if (rigidbody.useGravity)
            {
                ApplyGravity();
            }

            UpdateVelocity();
            Vector3 deltaPosition = velocity * Time.deltaTime;
            Vector3 moveAlongGround = rigidbody.useGravity ? new Vector3(groundNormal.y, -groundNormal.x, 0) : Vector3.right;

            if (!rigidbody.constraints.HasFlag(RigidbodyConstraints.FreezePositionZ))
            {
                PerformMovement(new Vector3(0, 0, deltaPosition.z), false);
            }
            else
            {
                Vector3 horizontalMove = moveAlongGround * deltaPosition.x;
                PerformMovement(horizontalMove, false);
            }

            Vector3 verticalMove = Vector3.up * deltaPosition.y;
            PerformMovement(verticalMove, true);
        }

        void ApplyGravity()
        {
            velocity += gravityModifier * Physics.gravity * Time.deltaTime;
        }

        void UpdateVelocity()
        {
            velocity.x = targetVelocity.x;

            if (!rigidbody.constraints.HasFlag(RigidbodyConstraints.FreezePositionZ))
            {
                velocity.z = targetVelocity.z;
            }
            else
            {
                velocity.z = 0;
            }
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
