#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using NeonLadder.Dialog;

namespace NeonLadder.Dialog.Editor
{
    /// <summary>
    /// Menu items for creating dialogue system assets and sample data
    /// </summary>
    public static class DialogueSystemMenuItems
    {
        [MenuItem("Assets/Create/NeonLadder/Dialogue Scene Configuration", priority = 1)]
        public static void CreateDialogueSceneConfiguration()
        {
            CreateScriptableObjectAsset<DialogueSceneConfiguration>("New Dialogue Scene");
        }

        [MenuItem("Assets/Create/NeonLadder/Dialogue Choice Database", priority = 2)]
        public static void CreateDialogueChoiceDatabase()
        {
            var database = CreateScriptableObjectAsset<DialogueChoiceDatabase>("New Dialogue Choice Database");
            PopulateSampleChoiceData(database);
        }

        [MenuItem("Assets/Create/NeonLadder/Sample Boss Dialogue Scene", priority = 10)]
        public static void CreateSampleBossDialogueScene()
        {
            var config = CreateScriptableObjectAsset<DialogueSceneConfiguration>("Sample Boss Dialogue");
            PopulateSampleBossData(config);
        }

        [MenuItem("Assets/Create/NeonLadder/Sample NPC Dialogue Scene", priority = 11)]
        public static void CreateSampleNPCDialogueScene()
        {
            var config = CreateScriptableObjectAsset<DialogueSceneConfiguration>("Sample NPC Dialogue");
            PopulateSampleNPCData(config);
        }

        private static T CreateScriptableObjectAsset<T>(string defaultName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (System.IO.Path.GetExtension(path) != "")
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + defaultName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }

        private static void PopulateSampleChoiceData(DialogueChoiceDatabase database)
        {
            database.databaseName = "Sample Boss Encounters";
            database.version = "1.0";

            // Create boss encounters category
            var bossCategory = new DialogueChoiceCategory
            {
                categoryName = "BossEncounters",
                description = "Dialogue choices for boss battles"
            };

            // Wrath boss choices
            var wrathChoices = new DialogueChoiceSet
            {
                choiceSetName = "WrathDialogue",
                description = "Player responses to Wrath's intimidation",
                tags = new string[] { "boss", "wrath", "defiant", "combat" }
            };

            wrathChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "I won't back down from you!",
                chineseSimplified = "我不会退缩的！",
                urdu = "میں تم سے نہیں ڈروں گا!",
                consequence = "Increases combat readiness",
                requiredSkill = SkillType.Rhetoric,
                requiredLevel = 0,
                isEnabled = true
            });

            wrathChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "Your anger blinds you to reason.",
                chineseSimplified = "你的愤怒蒙蔽了你的理智。",
                urdu = "تمہارا غصہ تمہیں عقل سے اندھا کر دیتا ہے۔",
                consequence = "Attempts philosophical approach",
                requiredSkill = SkillType.Intuition,
                requiredLevel = 2,
                isEnabled = true
            });

            wrathChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "Let's settle this quickly.",
                chineseSimplified = "让我们快速解决这个问题。",
                urdu = "آؤ اسے جلدی حل کرتے ہیں۔",
                consequence = "Initiates immediate combat",
                requiredSkill = SkillType.TimeControl,
                requiredLevel = 1,
                isEnabled = true
            });

            bossCategory.choiceSets.Add(wrathChoices);

            // Pride boss choices
            var prideChoices = new DialogueChoiceSet
            {
                choiceSetName = "PrideDialogue",
                description = "Player responses to Pride's arrogance",
                tags = new string[] { "boss", "pride", "humble", "wisdom" }
            };

            prideChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "True strength doesn't need to boast.",
                chineseSimplified = "真正的力量不需要夸耀。",
                urdu = "حقیقی طاقت کو فخر کی ضرورت نہیں۔",
                consequence = "Challenges Pride's worldview",
                requiredSkill = SkillType.Charisma,
                requiredLevel = 3,
                isEnabled = true
            });

            prideChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "I've seen greater beings than you fall.",
                chineseSimplified = "我见过比你更伟大的存在倒下。",
                urdu = "میں نے تم سے بڑے مخلوقات کو گرتے دیکھا ہے۔",
                consequence = "Intimidation attempt",
                requiredSkill = SkillType.SpaceshipSynergy,
                requiredLevel = 2,
                isEnabled = true
            });

            bossCategory.choiceSets.Add(prideChoices);
            database.choiceCategories.Add(bossCategory);

            // Create NPC interactions category
            var npcCategory = new DialogueChoiceCategory
            {
                categoryName = "NPCInteractions",
                description = "Dialogue choices for NPC conversations"
            };

            var merchantChoices = new DialogueChoiceSet
            {
                choiceSetName = "MerchantBargaining",
                description = "Negotiating with merchant NPCs",
                tags = new string[] { "npc", "merchant", "trade", "negotiation" }
            };

            merchantChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "That price seems fair.",
                chineseSimplified = "这个价格似乎很公平。",
                urdu = "یہ قیمت منصفانہ لگتی ہے۔",
                consequence = "Accept standard pricing",
                requiredLevel = 0,
                isEnabled = true
            });

            merchantChoices.choices.Add(new MultiLanguageDialogueChoice
            {
                english = "I think we can negotiate something better.",
                chineseSimplified = "我想我们可以协商出更好的价格。",
                urdu = "مجھے لگتا ہے ہم بہتر بات چیت کر سکتے ہیں۔",
                consequence = "Attempt to bargain for better price",
                requiredSkill = SkillType.Rhetoric,
                requiredLevel = 1,
                isEnabled = true
            });

            npcCategory.choiceSets.Add(merchantChoices);
            database.choiceCategories.Add(npcCategory);

            EditorUtility.SetDirty(database);
            Debug.Log("✅ Populated sample dialogue choice data");
        }

        private static void PopulateSampleBossData(DialogueSceneConfiguration config)
        {
            config.sceneName = "Wrath Boss Encounter";
            config.description = "Sample setup for a boss dialogue scene with Wrath";

            // Left character (Boss)
            config.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Wrath",
                position = new Vector3(-2f, 0f, 0f),
                rotation = new Vector3(0f, 45f, 0f),
                scale = Vector3.one,
                actorName = "Wrath",
                role = DialogueCharacterRole.Boss,
                idleAnimationName = "Idle_Menacing",
                talkingAnimationName = "Talk_Aggressive"
            };

            // Right character (Player)
            config.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Protagonist",
                position = new Vector3(2f, 0f, 0f),
                rotation = new Vector3(0f, -45f, 0f),
                scale = Vector3.one,
                actorName = "Player",
                role = DialogueCharacterRole.Player,
                idleAnimationName = "Idle_Ready",
                talkingAnimationName = "Talk_Confident"
            };

            // Trigger setup
            config.triggerBoxSize = new Vector3(3f, 2f, 3f);
            config.triggerBoxOffset = Vector3.zero;
            config.triggerLayerMask = 1; // Default layer

            // Dialogue configuration
            config.interactionType = DialogueInteractionType.FullConversation;
            config.conversationName = "Wrath_FullDialogue";
            config.autoSetupPixelCrushersComponents = true;

            // Camera setup
            config.cameraSetup = new DialogueCameraSetup
            {
                position = new Vector3(0f, 1.5f, -4f),
                rotation = new Vector3(10f, 0f, 0f),
                fieldOfView = 60f,
                useCustomCamera = false
            };

            // Localization setup
            config.supportedLanguages = new string[] { "en", "zh-Hans", "ur" };
            config.textTableName = "BossDialogue";

            EditorUtility.SetDirty(config);
            Debug.Log("✅ Populated sample boss dialogue configuration");
        }

        private static void PopulateSampleNPCData(DialogueSceneConfiguration config)
        {
            config.sceneName = "Merchant Interaction";
            config.description = "Sample setup for NPC merchant dialogue";

            // Left character (Merchant)
            config.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Merchant",
                position = new Vector3(-1.5f, 0f, 0f),
                rotation = new Vector3(0f, 30f, 0f),
                scale = Vector3.one,
                actorName = "Merchant",
                role = DialogueCharacterRole.Merchant,
                idleAnimationName = "Idle_Friendly",
                talkingAnimationName = "Talk_Enthusiastic"
            };

            // Right character (Player)
            config.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Protagonist",
                position = new Vector3(1.5f, 0f, 0f),
                rotation = new Vector3(0f, -30f, 0f),
                scale = Vector3.one,
                actorName = "Player",
                role = DialogueCharacterRole.Player,
                idleAnimationName = "Idle_Casual",
                talkingAnimationName = "Talk_Polite"
            };

            // Trigger setup (smaller for NPC)
            config.triggerBoxSize = new Vector3(2f, 2f, 2f);
            config.triggerBoxOffset = Vector3.zero;
            config.triggerLayerMask = 1;

            // Dialogue configuration
            config.interactionType = DialogueInteractionType.PlayerChoice;
            config.conversationName = "Merchant_Trade";
            config.autoSetupPixelCrushersComponents = true;

            // Camera setup (closer for intimate conversation)
            config.cameraSetup = new DialogueCameraSetup
            {
                position = new Vector3(0f, 1.2f, -2.5f),
                rotation = new Vector3(5f, 0f, 0f),
                fieldOfView = 65f,
                useCustomCamera = false
            };

            // Localization setup
            config.supportedLanguages = new string[] { "en", "zh-Hans", "ur" };
            config.textTableName = "NPCDialogue";

            EditorUtility.SetDirty(config);
            Debug.Log("✅ Populated sample NPC dialogue configuration");
        }

        [MenuItem("Tools/NeonLadder/Validate All Dialogue Assets")]
        public static void ValidateAllDialogueAssets()
        {
            // Find all DialogueChoiceDatabase assets
            string[] guids = AssetDatabase.FindAssets("t:DialogueChoiceDatabase");
            int validatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath<DialogueChoiceDatabase>(path);
                
                if (database != null)
                {
                    database.ValidateDatabase();
                    validatedCount++;
                }
            }

            // Find all DialogueSceneConfiguration assets
            guids = AssetDatabase.FindAssets("t:DialogueSceneConfiguration");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<DialogueSceneConfiguration>(path);
                
                if (config != null)
                {
                    if (config.ValidateConfiguration(out string error))
                    {
                        Debug.Log($"✅ {config.name}: Configuration valid");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ {config.name}: {error}");
                    }
                    validatedCount++;
                }
            }

            Debug.Log($"🔍 Validated {validatedCount} dialogue assets");
        }

        [MenuItem("Tools/NeonLadder/Open Dialogue System Documentation")]
        public static void OpenDocumentation()
        {
            string docPath = System.IO.Path.Combine(Application.dataPath, "..", "documentation", "DialogueSystemSetupGuide.html");
            
            if (System.IO.File.Exists(docPath))
            {
                Application.OpenURL("file://" + docPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Documentation Not Found", 
                    "Could not find DialogueSystemSetupGuide.html in the documentation folder.", "OK");
            }
        }
    }
}
#endif