using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NeonLadder.UI;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Editor tool to automatically generate loading screen content from prefabs
    /// </summary>
    public class LoadingScreenContentGenerator
    {
        private const string ACTORS_PATH = "Assets/Prefabs/Actors";
        private const string ENEMIES_PATH = ACTORS_PATH + "/Enemies";
        private const string NPCS_PATH = ACTORS_PATH + "/NPCs";
        private const string PLAYERS_PATH = ACTORS_PATH + "/Players";

        [MenuItem("NeonLadder/Loading/Generate Loading Screen Content")]
        public static void GenerateLoadingScreenContent()
        {
            // Find or create the database
            var database = FindOrCreateDatabase();
            if (database == null)
            {
                Debug.LogError("[LoadingScreenContentGenerator] Failed to find or create LoadingScreenContentDatabase");
                return;
            }

            // Generate content based on database settings with basic defaults
            var settings = database.GetGenerationSettings();
            var basicDefaults = new LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults();
            var generatedModels = GenerateModelsFromPrefabsWithDefaults(settings, basicDefaults);

            // Update database
            database.RegenerateAutoModels(generatedModels);

            Debug.Log($"[LoadingScreenContentGenerator] Generated {generatedModels.Count} loading screen models and saved to database");

            // Save assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generate models with custom defaults (called from the advanced window)
        /// </summary>
        public static List<Loading3DModel> GenerateModelsFromPrefabsWithDefaults(LoadingScreenGenerationSettings settings, LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults defaults)
        {
            var models = new List<Loading3DModel>();

            // Minor Enemies
            if (settings.includeMinorEnemies)
            {
                var categoryDefaults = defaults.useMinorEnemyOverrides ? defaults.minorEnemyDefaults : null;
                models.AddRange(GenerateModelsFromFolder(ENEMIES_PATH + "/Minor", "Minor Enemy", defaults, categoryDefaults));
            }

            // Major Enemies
            if (settings.includeMajorEnemies)
            {
                var categoryDefaults = defaults.useMajorEnemyOverrides ? defaults.majorEnemyDefaults : null;
                models.AddRange(GenerateModelsFromFolder(ENEMIES_PATH + "/Major", "Major Enemy", defaults, categoryDefaults));
            }

            // Boss Enemies
            if (settings.includeBossEnemies)
            {
                var categoryDefaults = defaults.useBossOverrides ? defaults.bossDefaults : null;
                models.AddRange(GenerateModelsFromFolder(ENEMIES_PATH + "/Boss", "Boss", defaults, categoryDefaults));
            }

            // Boss Transformations
            if (settings.includeBossTransformations)
            {
                var categoryDefaults = defaults.useBossTransformationOverrides ? defaults.bossTransformationDefaults : null;
                models.AddRange(GenerateModelsFromFolder(ENEMIES_PATH + "/Boss/Transformations", "Boss Transformation", defaults, categoryDefaults));
            }

            // NPCs
            if (settings.includeNPCs)
            {
                var categoryDefaults = defaults.useNPCOverrides ? defaults.npcDefaults : null;
                models.AddRange(GenerateModelsFromFolder(NPCS_PATH, "NPC", defaults, categoryDefaults));
            }

            // Players
            if (settings.includePlayers)
            {
                var categoryDefaults = defaults.usePlayerOverrides ? defaults.playerDefaults : null;
                models.AddRange(GenerateModelsFromFolder(PLAYERS_PATH, "Hero", defaults, categoryDefaults));
            }

            return models;
        }

        public static LoadingScreenContentDatabase FindOrCreateDatabase()
        {
            // Look for existing database
            string[] guids = AssetDatabase.FindAssets("t:LoadingScreenContentDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LoadingScreenContentDatabase>(path);
            }

            // Create new database
            var database = ScriptableObject.CreateInstance<LoadingScreenContentDatabase>();

            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string assetPath = resourcesPath + "/LoadingScreenContentDatabase.asset";
            AssetDatabase.CreateAsset(database, assetPath);

            Debug.Log($"[LoadingScreenContentGenerator] Created new LoadingScreenContentDatabase at {assetPath}");

            return database;
        }

        // Legacy method for backward compatibility
        private static List<Loading3DModel> GenerateModelsFromPrefabs(LoadingScreenGenerationSettings settings)
        {
            var basicDefaults = new LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults();
            return GenerateModelsFromPrefabsWithDefaults(settings, basicDefaults);
        }

        // Legacy method for backward compatibility
        private static List<Loading3DModel> GenerateModelsFromFolder(string folderPath, string category)
        {
            var basicDefaults = new LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults();
            return GenerateModelsFromFolder(folderPath, category, basicDefaults, null);
        }

        private static List<Loading3DModel> GenerateModelsFromFolder(string folderPath, string category, LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults globalDefaults, LoadingScreenGenerationWindow.LoadingModelDefaults categoryDefaults)
        {
            var models = new List<Loading3DModel>();

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[LoadingScreenContentGenerator] Folder not found: {folderPath}");
                return models;
            }

            // Find all prefabs in the folder
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null && IsValidLoadingScreenPrefab(prefab, category, prefabPath))
                {
                    var model = CreateModelFromPrefab(prefab, category, globalDefaults, categoryDefaults);
                    models.Add(model);
                }
            }

            Debug.Log($"[LoadingScreenContentGenerator] Generated {models.Count} models from {category} folder: {folderPath}");
            return models;
        }

        private static bool IsValidLoadingScreenPrefab(GameObject prefab, string category, string prefabPath)
        {
            // Skip script container prefabs and utility prefabs
            if (prefab.name.Contains("Scripts") || prefab.name.Contains("Group") || prefab.name.Contains("Transformations"))
            {
                return false;
            }

            // For Boss category, exclude transformation prefabs (they should only appear in Boss Transformation category)
            if (category == "Boss" && prefabPath.Contains("/Transformations/"))
            {
                return false;
            }

            // Ensure it has a renderer (visual model)
            return prefab.GetComponentInChildren<Renderer>() != null;
        }

        // Legacy method for backward compatibility
        private static Loading3DModel CreateModelFromPrefab(GameObject prefab, string category)
        {
            var basicDefaults = new LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults();
            return CreateModelFromPrefab(prefab, category, basicDefaults, null);
        }

        private static Loading3DModel CreateModelFromPrefab(GameObject prefab, string category, LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults globalDefaults, LoadingScreenGenerationWindow.LoadingModelDefaults categoryDefaults)
        {
            var model = new Loading3DModel();

            // Basic setup
            model.enabled = true;
            model.modelName = CleanPrefabName(prefab.name);
            model.modelPrefab = prefab;

            // Apply defaults - category overrides global
            if (categoryDefaults != null)
            {
                model.displayScale = categoryDefaults.displayScale;
                model.positionOffset = categoryDefaults.positionOffset;
                model.cameraDistanceOverride = categoryDefaults.cameraDistanceOverride;
                model.disableAnimator = categoryDefaults.disableAnimator;
                model.animatorParameterName = categoryDefaults.animatorParameterName;
                model.animationValue = categoryDefaults.animationValue;
            }
            else
            {
                model.displayScale = globalDefaults.displayScale;
                model.positionOffset = Vector3.zero; // Global defaults don't have position offset
                model.cameraDistanceOverride = globalDefaults.cameraDistanceOverride;
                model.disableAnimator = globalDefaults.disableAnimator;
                model.animatorParameterName = globalDefaults.animatorParameterName;
                model.animationValue = globalDefaults.animationValue;
            }

            Debug.Log($"[LoadingScreenContentGenerator] Created model '{model.modelName}' with scale {model.displayScale} and position offset {model.positionOffset}");

            // Generate contextual loading tip
            var tip = GenerateLoadingTip(prefab.name, category);
            model.loadingTips = new List<LoadingTip> { tip };

            return model;
        }

        private static string CleanPrefabName(string prefabName)
        {
            // Clean up prefab name for display
            return prefabName.Replace("_", " ").Replace("-", " ");
        }

        private static LoadingTip GenerateLoadingTip(string prefabName, string category)
        {
            var tip = new LoadingTip();
            tip.languageCode = "en";
            tip.title = $"About {CleanPrefabName(prefabName)}";
            tip.description = GenerateContextualDescription(prefabName, category);

            return tip;
        }

        private static string GenerateContextualDescription(string prefabName, string category)
        {
            string cleanName = CleanPrefabName(prefabName).ToLower();

            // Category-specific descriptions
            switch (category)
            {
                case "Minor Enemy":
                    return GenerateMinorEnemyDescription(cleanName);

                case "Major Enemy":
                    return GenerateMajorEnemyDescription(cleanName);

                case "Boss":
                    return GenerateBossDescription(cleanName);

                case "Boss Transformation":
                    return GenerateBossTransformationDescription(cleanName);

                case "Hero":
                    return GenerateHeroDescription(cleanName);

                case "NPC":
                    return GenerateNPCDescription(cleanName);

                default:
                    return $"A {category.ToLower()} encountered in the depths of the Neon Ladder.";
            }
        }

        private static string GenerateMinorEnemyDescription(string name)
        {
            if (name.Contains("knight"))
                return "Corrupted warriors who once served noble causes, now twisted by the ladder's dark influence.";

            if (name.Contains("skeleton"))
                return "Restless bones animated by malevolent energy, forever guarding forgotten secrets.";

            if (name.Contains("spider"))
                return "Venomous arachnids that have grown large in the ladder's toxic environment.";

            if (name.Contains("rat"))
                return "Mutated rodents that scurry through the ladder's shadows, more dangerous than they appear.";

            if (name.Contains("bee"))
                return "Aggressive insects whose stings carry otherworldly toxins.";

            if (name.Contains("fishman"))
                return "Aquatic humanoids that have adapted to the ladder's flooded lower levels.";

            if (name.Contains("golem"))
                return "Ancient constructs powered by mysterious crystals, slow but nearly indestructible.";

            if (name.Contains("mimic"))
                return "Deceptive creatures that disguise themselves as treasure chests and other objects.";

            if (name.Contains("wizard") || name.Contains("naga"))
                return "Serpentine spellcasters who command dark magic and ancient curses.";

            if (name.Contains("crab"))
                return "Armored crustaceans with powerful claws, adapted to the ladder's harsh conditions.";

            if (name.Contains("salamander"))
                return "Fire-breathing amphibians that thrive in the ladder's heated chambers.";

            if (name.Contains("werewolf"))
                return "Shape-shifting predators that hunt in packs during the ladder's eternal twilight.";

            if (name.Contains("stingray"))
                return "Floating creatures that glide through the air, delivering electric shocks to unwary climbers.";

            return "A common but dangerous creature that inhabits the lower levels of the Neon Ladder.";
        }

        private static string GenerateMajorEnemyDescription(string name)
        {
            if (name.Contains("dragon"))
                return "A fearsome predator combining the fury of dragons with the cunning of sharks.";

            if (name.Contains("gorilla"))
                return "Massive primates enhanced with cybernetic implants, possessing incredible strength.";

            if (name.Contains("turtle"))
                return "Ancient armored beasts whose shells can deflect even the strongest attacks.";

            if (name.Contains("scorpion"))
                return "Giant arachnids with venomous stingers capable of paralyzing their prey.";

            if (name.Contains("lizard"))
                return "Massive reptilian predators that rule the middle tiers of the ladder.";

            if (name.Contains("crab"))
                return "Enormous crustaceans with armor-piercing claws and incredible resilience.";

            if (name.Contains("frog"))
                return "Airborne amphibians that use their toxic breath to disable climbing equipment.";

            return "A powerful guardian that protects the ladder's most valuable treasures.";
        }

        private static string GenerateBossDescription(string name)
        {
            if (name.Contains("pride"))
                return "The embodiment of arrogance and vanity, this boss demands absolute reverence.";

            if (name.Contains("wrath"))
                return "Pure rage given form, channeling the anger of all who have fallen in the ladder.";

            if (name.Contains("greed"))
                return "A hoarder of souls and treasure, never satisfied despite endless accumulation.";

            if (name.Contains("envy"))
                return "Consumed by jealousy, this entity covets everything others possess.";

            if (name.Contains("lust"))
                return "A seductive and dangerous force that corrupts through desire and temptation.";

            if (name.Contains("gluttony"))
                return "An insatiable devourer that consumes everything in its path without end.";

            if (name.Contains("sloth"))
                return "The master of stagnation, seeking to trap climbers in eternal complacency.";

            if (name.Contains("hara"))
                return "A mysterious entity whose true nature remains hidden in shadow and legend.";

            return "One of the Seven Deadly Sins made manifest, a test of your worthiness to ascend.";
        }

        private static string GenerateBossTransformationDescription(string name)
        {
            if (name.Contains("anglerox"))
                return "A deep-sea predator that lures prey with hypnotic bioluminescent displays.";

            if (name.Contains("arack"))
                return "A massive arachnid with legs that can pierce through the strongest armor.";

            if (name.Contains("ceratoferox"))
                return "A horned beast whose charge can shatter reinforced barriers.";

            if (name.Contains("gobbler"))
                return "A ravenous creature that grows stronger with each victim it devours.";

            if (name.Contains("onyscidus"))
                return "A razor-clawed predator that strikes with lightning-fast precision.";

            if (name.Contains("rapax"))
                return "A swift hunter that tears through enemies with relentless aggression.";

            if (name.Contains("ursacetus"))
                return "A bear-whale hybrid that dominates both land and water environments.";

            return "An evolved form of the Seven Deadly Sins, representing their ultimate power.";
        }

        private static string GenerateHeroDescription(string name)
        {
            if (name.Contains("kaoru"))
                return "A skilled warrior who has mastered the art of ascending the Neon Ladder.";

            return "A brave soul who dares to challenge the mysteries of the Neon Ladder.";
        }

        private static string GenerateNPCDescription(string name)
        {
            if (name.Contains("aria"))
                return "A mysterious figure who provides guidance to lost climbers.";

            if (name.Contains("ellie"))
                return "A knowledgeable merchant who trades in rare ladder artifacts.";

            return "A helpful resident of the ladder who assists brave climbers on their journey.";
        }
    }
}