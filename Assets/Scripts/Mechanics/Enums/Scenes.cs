namespace NeonLadder.Mechanics.Enums
{
    /// <summary>
    /// Nested static class structure for scene organization with better IntelliSense support.
    /// Uses const strings for compile-time constants and static readonly arrays for iteration.
    /// Pattern: Each category has const string members and a static All array.
    /// </summary>
    public static class Scenes
    {
        /// <summary>
        /// Core system scenes (Title, Staging, etc.)
        /// </summary>
        public static class Core
        {
            public const string Unknown = "Unknown";
            public const string Title = "Title";
            public const string Staging = "Staging";
            public const string Start = "Start";
            public const string MetaShop = "MetaShop";
            public const string PermaShop = "PermaShop";
            public const string Credits = "Credits";
            public const string Test = "Test";
            
            public static readonly string[] All = new[]
            {
                Unknown,
                Title,
                Staging,
                Start,
                MetaShop,
                PermaShop,
                Credits,
                Test
            };
        }
        /// <summary>
        /// Test scenes
        /// </summary>
        public static class Test
        {
            public const string BossBrawl = "BossBrawl";
            public const string MobFight = "MobFight";

            public static readonly string[] All = new[]
            {
                MobFight,
                BossBrawl
            };
        }

        /// <summary>
        /// Boss arena scenes (Generated/BossArenas/)
        /// </summary>
        public static class Boss
        {
            public const string Banquet = "Banquet";
            public const string Cathedral = "Cathedral";
            public const string Finale = "Finale";
            public const string Garden = "Garden";
            public const string Lounge = "Lounge";
            public const string Mirage = "Mirage";
            public const string Necropolis = "Necropolis";
            public const string Vault = "Vault";
            
            public static readonly string[] All = new[]
            {
                Banquet,
                Cathedral,
                Finale,
                Garden,
                Lounge,
                Mirage,
                Necropolis,
                Vault
            };
        }
        
        /// <summary>
        /// Connection scenes between areas (Generated/Connections/)
        /// </summary>
        public static class Connection
        {
            public const string Banquet_Connection1 = "Banquet_Connection1";
            public const string Banquet_Connection2 = "Banquet_Connection2";
            public const string Cathedral_Connection1 = "Cathedral_Connection1";
            public const string Cathedral_Connection2 = "Cathedral_Connection2";
            public const string Finale_Connection1 = "Finale_Connection1";
            public const string Finale_Connection2 = "Finale_Connection2";
            public const string Garden_Connection1 = "Garden_Connection1";
            public const string Garden_Connection2 = "Garden_Connection2";
            public const string Lounge_Connection1 = "Lounge_Connection1";
            public const string Lounge_Connection2 = "Lounge_Connection2";
            public const string Mirage_Connection1 = "Mirage_Connection1";
            public const string Mirage_Connection2 = "Mirage_Connection2";
            public const string Necropolis_Connection1 = "Necropolis_Connection1";
            public const string Necropolis_Connection2 = "Necropolis_Connection2";
            public const string Vault_Connection1 = "Vault_Connection1";
            public const string Vault_Connection2 = "Vault_Connection2";
            
            public static readonly string[] All = new[]
            {
                Banquet_Connection1,
                Banquet_Connection2,
                Cathedral_Connection1,
                Cathedral_Connection2,
                Finale_Connection1,
                Finale_Connection2,
                Garden_Connection1,
                Garden_Connection2,
                Lounge_Connection1,
                Lounge_Connection2,
                Mirage_Connection1,
                Mirage_Connection2,
                Necropolis_Connection1,
                Necropolis_Connection2,
                Vault_Connection1,
                Vault_Connection2
            };
        }
        
        /// <summary>
        /// Service scenes for shops, rest areas, etc. (Generated/Services/)
        /// </summary>
        public static class Service
        {
            public const string Merchant = "Merchant";
            public const string MysteriousAltar = "MysteriousAltar";
            public const string Portal = "Portal";
            public const string RestArea = "RestArea";
            public const string RiddleRoom = "RiddleRoom";
            public const string Shop = "Shop";
            public const string Shrine = "Shrine";
            public const string TreasureRoom = "TreasureRoom";
            
            public static readonly string[] All = new[]
            {
                Merchant,
                MysteriousAltar,
                Portal,
                RestArea,
                RiddleRoom,
                Shop,
                Shrine,
                TreasureRoom
            };
        }
        
        /// <summary>
        /// Legacy scenes for testing (not production-ready)
        /// </summary>
        public static class Legacy
        {
            public const string BossBrawl = "BossBrawl";
            public const string SidePath1 = "SidePath1";
            public const string MainPath3 = "MainPath3";
            public const string MainPath2 = "MainPath2";
            public const string MainPath1 = "MainPath1";
            
            public static readonly string[] All = new[]
            {
                BossBrawl,
                SidePath1,
                MainPath3,
                MainPath2,
                MainPath1
            };
        }
        
        /// <summary>
        /// Package scenes purchased from Unity Asset Store (Assets/Scenes/PackageScenes/)
        /// </summary>
        public static class Packaged
        {
            public const string URP_SiegeOfPonthus = "URP_SiegeOfPonthus";
            public const string URP_AncientCathedral = "URP_AncientCathedral";
            public const string Medievil_Tavern = "Medievil_Tavern";
            public const string SpaceBurgers = "SpaceBurgers";
            public const string AbandonedFactory = "AbandonedFactory";
            public const string AbandonedPoolURP = "AbandonedPoolURP";
            public const string Cyberpunk_Room = "Cyberpunk_Room";
            public const string CyberpunkURPScene = "CyberpunkURPScene";
            public const string URP_Rooftop_Market = "URP_Rooftop_Market";
            public const string URP_RomanStreet = "URP_RomanStreet";
            public const string Showcase = "Showcase";
            public const string FC_URP_Scene = "FC_URP_Scene";
            public const string StandardizedOstrichBar = "StandardizedOstrichBar";
            public const string Recife_2050_Final = "Recife_2050_Final";
            
            public static readonly string[] All = new[]
            {
                URP_SiegeOfPonthus,
                URP_AncientCathedral,
                Medievil_Tavern,
                SpaceBurgers,
                AbandonedFactory,
                AbandonedPoolURP,
                Cyberpunk_Room,
                CyberpunkURPScene,
                URP_Rooftop_Market,
                URP_RomanStreet,
                Showcase,
                FC_URP_Scene,
                StandardizedOstrichBar,
                Recife_2050_Final
            };
        }
        
        /// <summary>
        /// Cutscene scenes for story and cinematics
        /// </summary>
        public static class Cutscene
        {
            public const string BossDefeated = "BossDefeated";
            public const string Death = "Death";
            public const string Credits = "Credits";
            public const string Title = "Title";

            public static readonly string[] All = new[]
            {
                BossDefeated,
                Death,
                Credits,
                Title
            };
        }
        
        /// <summary>
        /// Helper method to get all scene names across all categories
        /// </summary>
        public static string[] GetAllScenes()
        {
            var allScenes = new System.Collections.Generic.List<string>();
            allScenes.AddRange(Core.All);
            allScenes.AddRange(Boss.All);
            allScenes.AddRange(Connection.All);
            allScenes.AddRange(Service.All);
            allScenes.AddRange(Legacy.All);
            allScenes.AddRange(Packaged.All);
            allScenes.AddRange(Cutscene.All);
            return allScenes.ToArray();
        }
        
        /// <summary>
        /// Utility collections for scene management and transitions
        /// </summary>
        public static class SceneGroups
        {
            /// <summary>
            /// Scenes that are cutscenes and should be excluded from normal gameplay flow
            /// </summary>
            public static readonly string[] CutScenes = new[]
            {
                Cutscene.Death,
                Cutscene.BossDefeated,
                Cutscene.Credits,
                Cutscene.Title
            };
            
            /// <summary>
            /// Default scenes where player should spawn (typically safe areas)
            /// </summary>
            public static readonly string[] DefaultSpawnScenes = new[]
            {
                Core.Staging
            };
            
            /// <summary>
            /// Check if a scene is a cutscene that should be excluded from normal flow
            /// </summary>
            public static bool IsCutScene(string sceneName)
            {
                return System.Array.IndexOf(CutScenes, sceneName) >= 0;
            }
            
            /// <summary>
            /// Check if a scene is a default spawn location
            /// </summary>
            public static bool IsDefaultSpawnScene(string sceneName)
            {
                return System.Array.IndexOf(DefaultSpawnScenes, sceneName) >= 0;
            }
        }
    }
}