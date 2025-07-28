using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class KinematicObject : MonoBehaviour
    {
        public Player player { get; set; }
        public PlatformerModel model { get; private set; }
        public float minGroundNormalY = Constants.Physics.GroundDetection.MinNormalY;
        public float gravityModifier = 1f;
        public Vector3 velocity;
        public LayerMask layerMask;
        public bool IsGrounded { get; private set; }
        protected Vector3 targetVelocity;
        public Vector3 TargetVelocity 
        { 
            get => targetVelocity; 
            set => targetVelocity = value; 
        }
        protected Vector3 groundNormal;
        //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/Component.deprecated.cs
        protected new Rigidbody rigidbody;
        protected RaycastHit[] hitBuffer = new RaycastHit[16];
        protected const float minMoveDistance = Constants.Physics.GroundDetection.MinMoveDistance;
        protected const float shellRadius = Constants.Physics.GroundDetection.ShellRadius;
        public bool IsFacingLeft { get; set; }
        public bool IsFacingRight => !IsFacingLeft;

        [SerializeField]
        private bool isUsingMelee = true;
        public virtual bool IsUsingMelee
        {
            get => isUsingMelee;
            set => isUsingMelee = value;
        }
        public Animator Animator { get; private set; }
        public virtual float DeathAnimationDuration { get; set; }
        public virtual float AttackAnimationDuration { get; set; }
        public virtual float VictoryAnimationDuration { get; set; }
        public virtual float IdleAnimationDuration { get; set; }
        public virtual float GetHitAnimationDuration { get; set; }

        protected virtual float lastAttackTime { get; set; } = Constants.Physics.Combat.InitialLastAttackTime;

        private Dictionary<Animations, float> animationClipLengths;

        public void Bounce(float value)
        {
            velocity.y = value;
        }

        public void Teleport(Vector3 position)
        {
            transform.parent.position = position;
            velocity = Vector3.zero;
        }

        private void CacheAnimationClipLengths()
        {
            animationClipLengths = new Dictionary<Animations, float>();
            AnimationClip[] clips = Animator.runtimeAnimatorController.animationClips;

            foreach (var clip in clips)
            {
                if (System.Enum.TryParse(clip.name, out Animations animation))
                {
                    animationClipLengths[animation] = clip.length;
                }
            }
        }

        private float GetAnimationClipLength(Animations animation)
        {
            if (animationClipLengths.TryGetValue(animation, out float length))
            {
                return length;
            }
            Debug.LogWarning($"Animation {animation} not found on {Animator.name}");
            return 0f;
        }

        protected virtual void OnEnable()
        {
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
            rigidbody = GetComponentInParent<Rigidbody>();
            Animator = GetComponentInParent<Animator>();
            switch (this)
            {
                case FlyingMinor:
                case Minor:
                case FlyingMajor:
                case Major:
                case Boss:
                case Enemy:
                    rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
                case Player:
                    rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
                    break;
                default:
                    break;
            }

            //if (this.GetType() == typeof(Enemy))
            //{
            //    rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
            //}


            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
            }

            GuaranteeModelAndPlayer();
            CacheAnimationClipLengths();
            AttackAnimationDuration = GetAnimationClipLength(Animations.Attack1);
            DeathAnimationDuration = GetAnimationClipLength(Animations.Die);
            VictoryAnimationDuration = GetAnimationClipLength(Animations.Victory);
            IdleAnimationDuration = GetAnimationClipLength(Animations.Idle);
            GetHitAnimationDuration = GetAnimationClipLength(Animations.GetHit);
        }

        public IEnumerator PlayGetHitAnimation(bool stopEarly = false)
        {
            var animationParamName = "animation";
            Debug.Log("Getting hit...");
            if (transform.parent.name.Contains("Kaoru")) // kinematic's are on child game objects, but protagonist is more fleshed out in animations so far, what a hack, need to standardize.
            {
                animationParamName = $"locomotion_animation";
            }
            Animator.SetInteger(animationParamName, (int)Animations.GetHit);
            Debug.Log($"animation value: {Animator.GetInteger(animationParamName)}");
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitForSeconds(GetHitAnimationDuration);
            rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
            Animator.SetInteger(animationParamName, (int)Animations.Idle);
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
            //targetVelocity = Vector3.zero;
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

        public void Walk(float waitTimeInMs, float velocity, float duration, float directionDegrees)
        {
            EnableFreeRoam();
            StartCoroutine(WalkCoroutine(waitTimeInMs, velocity, duration, directionDegrees));
        }

        private IEnumerator WalkCoroutine(float waitTimeInMs, float velocity, float duration, float directionDegrees)
        {
            float waitTimeInSeconds = waitTimeInMs / 1000f;
            yield return new WaitForSeconds(waitTimeInSeconds);

            float endTime = Time.time + duration;
            float radians = directionDegrees * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians)).normalized;

            while (Time.time < endTime)
            {
                targetVelocity = direction * velocity;
                yield return null;
            }

            this.velocity = new Vector3(0, this.velocity.y, 0);
            DisableZMovement();
        }


        public void Orient()
        {
            transform.parent.rotation = Quaternion.Euler(0, !IsFacingLeft ? 90 : -90, 0);
        }

        public void EnableZMovement()
        {
            transform.parent.rotation = Quaternion.Euler(0, 0, 0);
            player.Actions.playerActionMap.Disable();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation |
                                    RigidbodyConstraints.FreezePositionX |
                                    RigidbodyConstraints.FreezePositionY;
        }

        public void DisableZMovement()
        {
            targetVelocity.z = 0;
            player.Actions.playerActionMap.Enable();
            rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
        }

        public void EnableFreeRoam()
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
