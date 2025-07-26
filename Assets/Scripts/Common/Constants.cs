using System;
using System.Collections.Generic;

namespace NeonLadder.Common
{
    [Serializable]
    public static class Constants
    {
        public static void UpdateSeed(string value)
        {
            Seed = value;
        }

        public static string Seed { get; set; } = string.Empty;
        public static string LastSeed { get; set; } = string.Empty;
        public static string ProtagonistModel = "Kaoru"; //"Hara";

        #region Player Unlocks etc, need to move to real storage
        public static List<string> DefeatedBosses { get; set; } = new List<string>();
        #endregion

        public static string Minimap = string.Empty;
        public static string PlayerWeapon => nameof(PlayerWeapon);

        #region Resources

        #region LootTables
        public static string MinorEnemyLootTablePath = "ScriptableObjects/MinorEnemyLootTable";
        public static string MajorEnemyLootTablePath = "ScriptableObjects/MajorEnemyLootTable";
        public static string BossEnemyLootTablePath = "ScriptableObjects/BossEnemyLootTable";
        #endregion

        #region Managers
        public static string LootPurchaseManagerPath = "Managers/LootPurchaseManager";
        #endregion

        #endregion

        public static float TransformationDurationInSeconds = 3.5f;


        #region Settings
        public static bool DisplayKeyPresses { get; set; } = true;
        public static bool DisplayAnimationDebugInfo { get; set; } = true;
        public static bool DisplayPlayerActionDebugInfo { get; set; } = true;
        public static bool DisplayControllerDebugInfo { get; set; } = true;
        public static bool DisplayTouchScreenDebugInfo { get; set;} = true;
        #endregion

        #region world
        public const float DefaultGravity = 9.81f;
        #endregion

        #region Camera
        public static float ZMovementCameraPivotDurationInSeconds = 3f;
        #endregion

        #region player
        public static int AttackDamage = 100;
        public static float AttackRange = 2.5f;
        public static float AttackSpeed = 1f;

        public const float DefaultMaxSpeed = 2f;
        public const float DefaultJumpTakeOffSpeed = 5f;
        public const float DefaultJumpModifier = 1.5f;
        public const float DefaultDeceleration = 0.5f;
        public const float DefaultCharacterScale = 1f;
        public const int DefaultMaxHealth = 100;
        public const int DefaultMaxStamina = 100;
        public const float DefaultKnockbackDuration = 0.25f; // Duration of the knockback effect
        public const float DefaultKnockbackSpeed = 2f;
        public const float DefaultKnockMultiplier = 1f;

        public const int PlayerLocomotionLayerIndex = 0; // Index for the locomotion layer
        public const int PlayerActionLayerIndex = 1; // Index for the action layer
        public const int MiscActionLayerIndex = 2; // Index for the action layer


        public static float JumpTakeOffSpeed { get; set; } = DefaultJumpTakeOffSpeed;
        public static float JumpModifier { get; set; } = DefaultJumpModifier;
        public static float JumpDeceleration { get; set; } = DefaultDeceleration;
        public static float CrouchScale { get; set; } = DefaultCharacterScale * 0.5f;
        public static float CutsceneProtagonistWalkSpeed { get; set; } = DefaultMaxSpeed * 0.4f;
        public static float CutsceneFinalBossWalkSpeed { get; set; } = DefaultMaxSpeed;
        public static int MaxHealth { get; set; } = DefaultMaxHealth;
        public static int MaxStamina { get; set; } = DefaultMaxStamina;

        #endregion

        #region actions
        /// <summary>
        /// The JumpCutOffFactor is a multiplier used to reduce the upward velocity of the player's character when the jump button is released before reaching the maximum jump height. This factor allows for variable jump heights, providing a more dynamic and responsive jumping mechanic in the game. Here's a detailed explanation and some sample values:
        /// </summary>
        public const float JumpCutOffFactor = 0.5f;
        public const float JumpDuration = 0.5f;
        public const float JumpStaminaCost = 2.0f;
        public const float WallJumpForce = 5.0f;

        public const float SprintDuration = 10f;
        public const float SprintCooldown = 1f;
        public const float SprintSpeedMultiplier = 3.5f;
        public const float SprintStaminaCost = 3f;

        public const float DashDuration = 0.6f;
        public const float DashCooldown = 1f;
        public const float DashSpeedMultiplier = 2f;
        public const float DashStaminaCost = 50f;

        public const float ClimbDuration = 0.8f;



        public const float CrouchStaminaCost = 25.0f;

        #endregion

        #region physics
        // Ground detection
        public const float MinGroundNormalY = 0.75f;
        public const float MinMoveDistance = 0.001f;
        public const float ShellRadius = 0.01f;
        
        // Movement thresholds
        public const float MovementThreshold = 0.1f;
        
        // Stamina regeneration
        public const float StaminaRegenInterval = 0.1f;
        public const float StaminaRegenAmount = 0.1f;
        
        // UI percentages
        public const float PercentageMultiplier = 100f;
        
        // Animation
        public const float AnimationIgnorePercentage = 0.35f;
        
        // Combat
        public const float DefaultAttackRange = 3.0f;
        public const float AttackRangeBoxColliderOffset = 1.0f;
        public const float AttackCooldown = 3.0f;
        public const float RetreatBuffer = 1.0f;
        public const int EnemyAttackDamage = 10;
        public const int RetreatSpeedDivider = 2;
        public const float DeathAnimationBuffer = 1f;
        public const float InitialLastAttackTime = -10f;
        
        // Flying enemy
        public const float FlyingEnemyAttackRange = 5f;
        
        // Damage numbers
        public const float DamageNumberYOffset = 1f;
        public const float CriticalHitScale = 1.5f;
        public const float NormalHitScale = 1f;
        
        // Projectiles
        public const float RaycastDistance = 100f;
        public const int ProjectileDamage = 10;
        public const float ProjectileLifetime = 2f;
        public const float ImpactEffectLifetime = 5f;
        
        // Collision
        public const float CollisionAvoidanceTimer = 0.6f;
        
        // Portal spawning
        public const float PortalSpawnInterval = 5f;
        public const float PortalDefaultLifetime = 0f;
        
        // Scene transitions
        public const float CelebrationDuration = 6.3f;
        public const float CameraZoomAmount = 5f;
        public const float SceneChangePlayerYPosition = 0.01f;
        
        // Cutscene movements
        public const float CutsceneWaitTimeMs = 500f;
        public const float CutsceneVelocity = 2f;
        public const float CutsceneDuration = 3f;
        public const float CutsceneRotationDegree = 90f;
        
        // Animation fly away
        public const float FlyAwayZDistance = 3.61f;
        public const float FlyAwayDuration = 30f;
        public const float FlyAwayFinalScale = 0f;
        
        // Audio
        public const float AudioTriggerTime = 0.5f;
        public const float AudioInitialTime = -1f;
        #endregion

        #region scenes
        public static List<string> MinorEnemyLevels = new List<string> { "Minor-Enemy-1", "Minor-Enemy-2", "Minor-Enemy-3" };
        public static List<string> MajorEnemyLevels = new List<string> { "Major-Enemy-1", "Major-Enemy-2", "Major-Enemy-3" };
        //public static List<string> Bosses = new List<string> { "Wrath", "Gluttony", "Pride", "Sloth", "Envy", "Lust", "Greed" };
        public static List<string> ShopLevels = new List<string> { "Shop-1", "Shop-2", "Shop-3" };
        public static List<string> MinorEnemies = new List<string> { "Chili", "Kiwi", "Eggy", "Langsat" };
        public static List<string> MajorEnemies = new List<string> { "BlackKnight", "Werewolf", "FlyingDemon" };
        #endregion
    }
}

