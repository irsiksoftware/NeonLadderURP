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
        public static class Physics
        {
            public static class GroundDetection
            {
                public const float MinNormalY = 0.75f;
                public const float MinMoveDistance = 0.001f;
                public const float ShellRadius = 0.01f;
            }
            
            public static class Movement
            {
                public const float ZAxisThreshold = 0.1f;
                public const int RetreatSpeedDivider = 2;
            }
            
            public static class Stamina
            {
                public const float RegenInterval = 0.1f;
                public const float RegenAmount = 0.1f;
            }
            
            public static class Combat
            {
                public const float DefaultAttackRange = 3.0f;
                public const float BoxColliderOffset = 1.0f;
                public const float AttackCooldown = 3.0f;
                public const float RetreatBuffer = 1.0f;
                public const int EnemyDamage = 10;
                public const float DeathAnimationBuffer = 1f;
                public const float InitialLastAttackTime = -10f;
                public const float FlyingEnemyRange = 5f;
            }
            
            public static class Projectiles
            {
                public const float RaycastDistance = 100f;
                public const int Damage = 10;
                public const float Lifetime = 2f;
                public const float ImpactEffectLifetime = 5f;
            }
            
            public static class Collision
            {
                public const float AvoidanceTimer = 0.6f;
            }
        }
        
        public static class UI
        {
            public const float PercentageMultiplier = 100f;
            
            public static class DamageNumbers
            {
                public const float YOffset = 1f;
                public const float CriticalHitScale = 1.5f;
                public const float NormalHitScale = 1f;
            }
        }
        
        public static class Animation
        {
            public const float IgnorePercentage = 0.35f;
            
            public static class FlyAway
            {
                public const float ZDistance = 3.61f;
                public const float Duration = 30f;
                public const float FinalScale = 0f;
            }
        }
        
        public static class Spawning
        {
            public static class Portal
            {
                public const float SpawnInterval = 5f;
                public const float DefaultLifetime = 0f;
            }
        }
        
        public static class SceneTransition
        {
            public const float CelebrationDuration = 6.3f;
            public const float CameraZoomAmount = 5f;
            public const float PlayerYPosition = 0.01f;
        }
        
        public static class Cutscene
        {
            public const float WaitTimeMs = 500f;
            public const float Velocity = 2f;
            public const float Duration = 3f;
            public const float RotationDegree = 90f;
        }
        
        public static class Audio
        {
            public const float TriggerTime = 0.5f;
            public const float InitialTime = -1f;
        }
        #endregion

        #region scenes_and_story
        public enum ChapterScenes
        {
            MinorEnemy1,
            MinorEnemy2, 
            MinorEnemy3,
            MajorEnemy1,
            MajorEnemy2,
            MajorEnemy3,
            Shop1,
            Shop2,
            Shop3
        }
        
        public enum EnemyTypes
        {
            // Minor enemies
            Chili,
            Kiwi,
            Eggy,
            Langsat,
            // Major enemies
            BlackKnight,
            Werewolf,
            FlyingDemon
        }
        
        public enum SevenDeadlySins
        {
            Wrath,
            Gluttony,
            Pride,
            Sloth,
            Envy,
            Lust,
            Greed
        }
        
        public static class SceneNames
        {
            public static readonly Dictionary<ChapterScenes, string> SceneMap = new Dictionary<ChapterScenes, string>
            {
                { ChapterScenes.MinorEnemy1, "Minor-Enemy-1" },
                { ChapterScenes.MinorEnemy2, "Minor-Enemy-2" },
                { ChapterScenes.MinorEnemy3, "Minor-Enemy-3" },
                { ChapterScenes.MajorEnemy1, "Major-Enemy-1" },
                { ChapterScenes.MajorEnemy2, "Major-Enemy-2" },
                { ChapterScenes.MajorEnemy3, "Major-Enemy-3" },
                { ChapterScenes.Shop1, "Shop-1" },
                { ChapterScenes.Shop2, "Shop-2" },
                { ChapterScenes.Shop3, "Shop-3" }
            };
            
            public static readonly Dictionary<EnemyTypes, string> EnemyMap = new Dictionary<EnemyTypes, string>
            {
                { EnemyTypes.Chili, "Chili" },
                { EnemyTypes.Kiwi, "Kiwi" },
                { EnemyTypes.Eggy, "Eggy" },
                { EnemyTypes.Langsat, "Langsat" },
                { EnemyTypes.BlackKnight, "BlackKnight" },
                { EnemyTypes.Werewolf, "Werewolf" },
                { EnemyTypes.FlyingDemon, "FlyingDemon" }
            };
            
            public static string GetSceneName(ChapterScenes scene) => SceneMap[scene];
            public static string GetEnemyName(EnemyTypes enemy) => EnemyMap[enemy];
            
            // Helper methods for backwards compatibility
            public static List<string> MinorEnemyLevels => new List<string> 
            { 
                GetSceneName(ChapterScenes.MinorEnemy1),
                GetSceneName(ChapterScenes.MinorEnemy2),
                GetSceneName(ChapterScenes.MinorEnemy3)
            };
            
            public static List<string> MajorEnemyLevels => new List<string>
            {
                GetSceneName(ChapterScenes.MajorEnemy1),
                GetSceneName(ChapterScenes.MajorEnemy2),
                GetSceneName(ChapterScenes.MajorEnemy3)
            };
            
            public static List<string> ShopLevels => new List<string>
            {
                GetSceneName(ChapterScenes.Shop1),
                GetSceneName(ChapterScenes.Shop2),
                GetSceneName(ChapterScenes.Shop3)
            };
            
            public static List<string> MinorEnemies => new List<string>
            {
                GetEnemyName(EnemyTypes.Chili),
                GetEnemyName(EnemyTypes.Kiwi),
                GetEnemyName(EnemyTypes.Eggy),
                GetEnemyName(EnemyTypes.Langsat)
            };
            
            public static List<string> MajorEnemies => new List<string>
            {
                GetEnemyName(EnemyTypes.BlackKnight),
                GetEnemyName(EnemyTypes.Werewolf),
                GetEnemyName(EnemyTypes.FlyingDemon)
            };
        }
        #endregion
    }
}

