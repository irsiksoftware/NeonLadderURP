using UnityEngine;

namespace NeonLadder.Models
{
    /// <summary>
    /// Constants and enums for action platformer game mechanics
    /// Provides readable constants for purchasable item effects and abilities
    /// </summary>
    public static class ActionPlatformerConstants
    {
        #region Ability Names - Used for GrantAbility effects
        
        /// <summary>
        /// Common action platformer abilities that can be granted through purchasable items
        /// </summary>
        public static class Abilities
        {
            // Movement Abilities
            public const string DoubleJump = "double_jump";
            public const string TripleJump = "triple_jump";
            public const string Dash = "dash";
            public const string AirDash = "air_dash";
            public const string WallJump = "wall_jump";
            public const string WallSlide = "wall_slide";
            public const string Glide = "glide";
            public const string GroundPound = "ground_pound";
            
            // Combat Abilities
            public const string Fireball = "fireball";
            public const string IceBlast = "ice_blast";
            public const string LightningBolt = "lightning_bolt";
            public const string ShieldBlock = "shield_block";
            public const string ChargedAttack = "charged_attack";
            public const string SpinAttack = "spin_attack";
            public const string Boomerang = "boomerang";
            
            // Utility Abilities
            public const string Grapple = "grapple";
            public const string Teleport = "teleport";
            public const string Invisibility = "invisibility";
            public const string SlowTime = "slow_time";
            public const string MagnetPull = "magnet_pull";
            public const string DoubleSize = "double_size";
            public const string HalfSize = "half_size";
        }
        
        #endregion
        
        #region Stat Properties - Used for StatBoost effects
        
        /// <summary>
        /// Player stats that can be boosted through purchasable items
        /// </summary>
        public static class Stats
        {
            // Health & Survival
            public const string Health = "health";
            public const string MaxHealth = "maxhealth";
            public const string Stamina = "stamina";
            public const string MaxStamina = "maxstamina";
            public const string DefenseRating = "defense";
            
            // Movement Stats
            public const string MoveSpeed = "speed";
            public const string JumpHeight = "jump_height";
            public const string JumpSpeed = "jump_speed";
            public const string Acceleration = "acceleration";
            public const string AirControl = "air_control";
            
            // Combat Stats
            public const string AttackDamage = "damage";
            public const string AttackSpeed = "attack_speed";
            public const string AttackRange = "attack_range";
            public const string CriticalChance = "crit_chance";
            public const string CriticalDamage = "crit_damage";
            
            // Collection & Economy
            public const string CoinMagnetRange = "coin_magnet";
            public const string DropRate = "drop_rate";
            public const string ExperienceMultiplier = "exp_multiplier";
            public const string CurrencyMultiplier = "currency_multiplier";
        }
        
        #endregion
        
        #region Custom Events - Used for CustomEvent effects
        
        /// <summary>
        /// Custom events that can be triggered through purchasable items
        /// </summary>
        public static class CustomEvents
        {
            // Currency & Economy
            public const string StartingMetaCurrency = "starting_meta_currency";
            public const string StartingPermaCurrency = "starting_perma_currency";
            public const string BonusLives = "bonus_lives";
            
            // Unlocks & Access
            public const string UnlockSecretLevel = "unlock_secret_level";
            public const string UnlockBossRush = "unlock_boss_rush";
            public const string UnlockNewGamePlus = "unlock_new_game_plus";
            
            // Special Effects
            public const string InvincibilityFrames = "invincibility_frames";
            public const string AutoSave = "auto_save";
            public const string SkipTutorial = "skip_tutorial";
            public const string UnlockDevMode = "unlock_dev_mode";
        }
        
        #endregion
        
        #region Item Rarity Colors
        
        /// <summary>
        /// Standard rarity color scheme for purchasable items
        /// </summary>
        public static class RarityColors
        {
            public static readonly Color Common = Color.white;
            public static readonly Color Uncommon = Color.green;
            public static readonly Color Rare = Color.blue;
            public static readonly Color Epic = Color.magenta;
            public static readonly Color Legendary = Color.yellow;
            public static readonly Color Mythic = new Color(1f, 0.5f, 0f); // Orange
            public static readonly Color Artifact = Color.red;
        }
        
        #endregion
        
        #region Currency Costs - Balanced for action platformer progression
        
        /// <summary>
        /// Balanced cost values for different item types
        /// </summary>
        public static class Costs
        {
            // Consumables (Meta Currency)
            public const int HealthPotion = 25;
            public const int StaminaPotion = 20;
            public const int ManaPotion = 30;
            
            // Temporary Upgrades (Meta Currency)
            public const int DamageBoost = 75;
            public const int SpeedBoost = 50;
            public const int DefenseBoost = 60;
            
            // Basic Abilities (Perma Currency)
            public const int DoubleJump = 150;
            public const int WallJump = 200;
            public const int Dash = 300;
            
            // Advanced Abilities (Perma Currency)
            public const int AirDash = 500;
            public const int Glide = 400;
            public const int Teleport = 750;
            
            // Permanent Upgrades (Perma Currency)
            public const int HealthUpgrade = 100;
            public const int SpeedUpgrade = 125;
            public const int DamageUpgrade = 175;
            
            // Utility Items (Perma Currency)
            public const int StartingGold = 200;
            public const int ExtraLife = 250;
            public const int CoinMagnet = 150;
        }
        
        #endregion
    }
}