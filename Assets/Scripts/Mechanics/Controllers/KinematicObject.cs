using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// Implements game physics for some in-game entity.
    /// </summary>
    public class KinematicObject : MonoBehaviour
    {
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

        public void Bounce(Vector3 dir)
        {
            velocity = dir;
        }

        public void Teleport(Vector3 position)
        {
            rigidbody.position = position;
            velocity *= 0;
            rigidbody.velocity *= 0;
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
            // Adjusted for 3D physics layer mask, removing the use of ContactFilter2D
        }

        protected virtual void Update()
        {
            targetVelocity = Vector3.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {
            // Implement character-specific velocity computation here
        }

        protected virtual void FixedUpdate()
        {
            velocity += gravityModifier * Physics.gravity * Time.deltaTime;
            velocity.x = targetVelocity.x;

            // Limit Z movement or adjust it according to your requirements
            velocity.z = 0;

            IsGrounded = false;

            Vector3 deltaPosition = velocity * Time.deltaTime;
            Vector3 moveAlongGround = new Vector3(groundNormal.y, -groundNormal.x, 0);
            Vector3 move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector3.up * deltaPosition.y;
            PerformMovement(move, true);
        }

        void PerformMovement(Vector3 move, bool yMovement)
        {
            float distance = move.magnitude;
            if (distance > minMoveDistance)
            {
                Vector3 rayOrigin = transform.position;
                Vector3 rayDirection = move.normalized;

                int count = Physics.RaycastNonAlloc(rayOrigin, rayDirection, hitBuffer, distance + shellRadius, layerMask);
                for (int i = 0; i < count; i++)
                {
                    Vector3 currentNormal = hitBuffer[i].normal;
                    if (currentNormal.y > minGroundNormalY)
                    {
                        IsGrounded = true;
                        if (yMovement)
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }
                    if (IsGrounded)
                    {
                        float projection = Vector3.Dot(velocity, currentNormal);
                        if (projection < 0)
                        {
                            velocity = velocity - projection * currentNormal;
                        }
                    }
                    else
                    {
                        velocity.x = 0;
                        velocity.y = Mathf.Min(velocity.y, 0);
                    }
                    float modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            rigidbody.position = rigidbody.position + move.normalized * distance;
        }
    }
}
