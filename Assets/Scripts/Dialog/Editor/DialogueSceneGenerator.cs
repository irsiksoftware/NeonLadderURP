#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PixelCrushers.DialogueSystem;
using NeonLadder.Dialog;

namespace NeonLadder.Dialog.Editor
{
    /// <summary>
    /// Editor utility to generate complete dialogue scenes from configuration
    /// </summary>
    public static class DialogueSceneGenerator
    {
        // Flag to suppress dialogs during tests
        private static bool suppressDialogs = false;
        
        /// <summary>
        /// Enable/disable dialog suppression for testing
        /// </summary>
        public static void SetDialogSuppression(bool suppress)
        {
            suppressDialogs = suppress;
        }
        
        /// <summary>
        /// Safe display dialog that works in batch mode and interactive mode
        /// </summary>
        private static void SafeDisplayDialog(string title, string message, string ok = "OK")
        {
            if (Application.isBatchMode || suppressDialogs)
            {
                Debug.LogWarning($"[DisplayDialog - {title}] {message}");
            }
            else
            {
                EditorUtility.DisplayDialog(title, message, ok);
            }
        }
        
        /// <summary>
        /// Generate a complete dialogue scene from configuration
        /// </summary>
        public static void GenerateScene(DialogueSceneConfiguration config)
        {
            if (!config.ValidateConfiguration(out string error))
            {
                SafeDisplayDialog("Configuration Error", error, "OK");
                return;
            }

            // Create scene GameObject
            GameObject sceneRoot = new GameObject(config.sceneName);
            Undo.RegisterCreatedObjectUndo(sceneRoot, "Create Dialogue Scene");

            // Generate components
            GenerateCharacters(sceneRoot, config);
            GenerateTriggerBox(sceneRoot, config);
            GenerateDialogueComponents(sceneRoot, config);
            GenerateCamera(sceneRoot, config);
            GenerateUI(sceneRoot, config);

            // Select the created object
            Selection.activeGameObject = sceneRoot;
            
            Debug.Log($"✅ Generated dialogue scene: {config.sceneName}");
        }

        private static void GenerateCharacters(GameObject parent, DialogueSceneConfiguration config)
        {
            // Create characters container
            GameObject charactersContainer = new GameObject("Characters");
            charactersContainer.transform.SetParent(parent.transform);

            // Left character
            if (config.leftCharacter != null && config.leftCharacter.characterPrefab != null)
            {
                GameObject leftChar = InstantiatePrefabSafe(config.leftCharacter.characterPrefab, charactersContainer);
                if (leftChar != null)
                {
                    leftChar.transform.localPosition = config.leftCharacter.position;
                    leftChar.transform.localRotation = Quaternion.Euler(config.leftCharacter.rotation);
                    leftChar.transform.localScale = config.leftCharacter.scale;
                    leftChar.name = $"{config.leftCharacter.characterName} (Left)";

                    // Add dialogue actor if needed
                    AddDialogueActor(leftChar, config.leftCharacter);
                }
            }

            // Right character
            if (config.rightCharacter != null && config.rightCharacter.characterPrefab != null)
            {
                GameObject rightChar = InstantiatePrefabSafe(config.rightCharacter.characterPrefab, charactersContainer);
                if (rightChar != null)
                {
                    rightChar.transform.localPosition = config.rightCharacter.position;
                    rightChar.transform.localRotation = Quaternion.Euler(config.rightCharacter.rotation);
                    rightChar.transform.localScale = config.rightCharacter.scale;
                    rightChar.name = $"{config.rightCharacter.characterName} (Right)";

                    // Add dialogue actor if needed
                    AddDialogueActor(rightChar, config.rightCharacter);
                }
            }
        }

        /// <summary>
        /// Safely instantiate a prefab, handling cases where the prefab might not exist (e.g., in test environments)
        /// </summary>
        private static GameObject InstantiatePrefabSafe(GameObject prefab, GameObject parent)
        {
            if (prefab == null)
                return null;
            
            try
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    if (parent != null)
                    {
                        instance.transform.SetParent(parent.transform);
                    }
                    return instance;
                }
                else
                {
                    throw new System.Exception("PrefabUtility returned null");
                }
            }
            catch (System.Exception)
            {
                // In test environments, try to instantiate as a regular GameObject clone
                try
                {
                    GameObject clone = Object.Instantiate(prefab);
                    if (clone != null)
                    {
                        if (parent != null)
                        {
                            clone.transform.SetParent(parent.transform);
                        }
                        return clone;
                    }
                    else
                    {
                        throw new System.Exception("Object.Instantiate returned null");
                    }
                }
                catch (System.Exception)
                {
                    GameObject placeholder = new GameObject(prefab.name + "_TestPlaceholder");
                    if (parent != null)
                    {
                        placeholder.transform.SetParent(parent.transform);
                    }
                    return placeholder;
                }
            }
        }

        private static void AddDialogueActor(GameObject character, DialogueCharacterConfig config)
        {
            if (character == null || config == null)
                return;
            
            try
            {
                // Get or add Dialogue Actor component
                var actor = character.GetComponent<DialogueActor>();
                if (actor == null)
                {
                    actor = character.AddComponent<DialogueActor>();
                }
                
                // Configure the actor name
                if (!string.IsNullOrEmpty(config.actorName))
                {
                    actor.actor = config.actorName;
                }

                // Get or add animator if specified
                if (config.animatorController != null)
                {
                    var animator = character.GetComponent<Animator>();
                    if (animator == null)
                    {
                        animator = character.AddComponent<Animator>();
                    }
                    animator.runtimeAnimatorController = config.animatorController;
                }

                // Get or add audio source if specified
                if (config.voiceClips != null && config.voiceClips.Length > 0)
                {
                    var audioSource = character.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = character.AddComponent<AudioSource>();
                    }
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0.5f; // 3D audio
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to add dialogue actor components to {character.name}: {ex.Message}");
            }
        }

        private static void GenerateTriggerBox(GameObject parent, DialogueSceneConfiguration config)
        {
            if (parent == null || config == null)
                return;
            
            try
            {
                // Create trigger GameObject
                GameObject trigger = new GameObject("Dialogue Trigger");
                trigger.transform.SetParent(parent.transform);
                trigger.transform.localPosition = config.triggerBoxOffset;

                // Add BoxCollider as trigger
                BoxCollider boxCollider = trigger.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.size = config.triggerBoxSize;

                // Add dialogue trigger component
                var dialogueTrigger = trigger.AddComponent<DialogueSystemTrigger>();
                dialogueTrigger.trigger = DialogueSystemTriggerEvent.OnTriggerEnter;
                if (!string.IsNullOrEmpty(config.conversationName))
                {
                    dialogueTrigger.conversation = config.conversationName;
                }

                // Configure layer mask
                var layerMask = config.triggerLayerMask;
                // Note: DialogueSystemTrigger uses tags, so we'd need to configure that

                Debug.Log($"Created dialogue trigger for conversation: {config.conversationName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to create dialogue trigger: {ex.Message}");
            }
        }

        private static void GenerateDialogueComponents(GameObject parent, DialogueSceneConfiguration config)
        {
            // Add dialogue system components container
            GameObject dialogueContainer = new GameObject("Dialogue System");
            dialogueContainer.transform.SetParent(parent.transform);

            // Add BossBanterManager if needed
            if (config.interactionType == DialogueInteractionType.SimpleBanter)
            {
                var banterManager = dialogueContainer.AddComponent<BossBanterManager>();
                Debug.Log("Added BossBanterManager component");
            }

            // Add ProtagonistDialogueSystem if needed
            if (config.interactionType == DialogueInteractionType.PlayerChoice || 
                config.interactionType == DialogueInteractionType.FullConversation)
            {
                var protagonistSystem = dialogueContainer.AddComponent<ProtagonistDialogueSystem>();
                Debug.Log("Added ProtagonistDialogueSystem component");
            }

            // Add DialogueSystemController if not present in scene
            if (Object.FindFirstObjectByType<DialogueSystemController>() == null && config.autoSetupPixelCrushersComponents)
            {
                var dialogueSystemController = dialogueContainer.AddComponent<DialogueSystemController>();
                Debug.Log("Added DialogueSystemController - don't forget to assign the dialogue database!");
            }
        }

        private static void GenerateCamera(GameObject parent, DialogueSceneConfiguration config)
        {
            if (config.cameraSetup == null)
            {
                Debug.LogWarning("Camera setup is null, skipping camera generation");
                return;
            }
            
            if (!config.cameraSetup.useCustomCamera)
            {
                // Create simple camera setup
                GameObject cameraContainer = new GameObject("Dialogue Camera");
                cameraContainer.transform.SetParent(parent.transform);
                cameraContainer.transform.localPosition = config.cameraSetup.position;
                cameraContainer.transform.localRotation = Quaternion.Euler(config.cameraSetup.rotation);

                Camera camera = cameraContainer.AddComponent<Camera>();
                camera.fieldOfView = config.cameraSetup.fieldOfView;
                camera.enabled = false; // Start disabled, enable during dialogue

                Debug.Log("Created dialogue camera");
            }
            else if (config.cameraSetup.customCameraPrefab != null)
            {
                // Instantiate custom camera prefab
                GameObject customCamera = InstantiatePrefabSafe(config.cameraSetup.customCameraPrefab, parent);
                if (customCamera != null)
                {
                    customCamera.name = "Custom Dialogue Camera";
                    Debug.Log("Added custom dialogue camera");
                }
            }
        }

        private static void GenerateUI(GameObject parent, DialogueSceneConfiguration config)
        {
            if (config.dialogueUIPrefab != null)
            {
                GameObject ui = InstantiatePrefabSafe(config.dialogueUIPrefab, parent);
                if (ui != null)
                {
                    ui.name = "Dialogue UI";
                    Debug.Log("Added dialogue UI prefab");
                }
            }
            else
            {
                // Create basic UI setup
                GameObject uiContainer = new GameObject("Dialogue UI");
                uiContainer.transform.SetParent(parent.transform);

                // Add Canvas
                Canvas canvas = uiContainer.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // High priority for dialogue

                // Add Canvas Scaler
                var canvasScaler = uiContainer.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);

                // Add GraphicRaycaster
                uiContainer.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                Debug.Log("Created basic dialogue UI canvas");
            }
        }

        [MenuItem("NeonLadder/Dialogue/Generate Dialogue Scene")]
        public static void GenerateDialogueSceneFromMenu()
        {
            // Find selected DialogueSceneConfiguration
            var selectedObject = Selection.activeObject as DialogueSceneConfiguration;
            if (selectedObject != null)
            {
                GenerateScene(selectedObject);
            }
            else
            {
                SafeDisplayDialog("No Configuration Selected", 
                    "Please select a DialogueSceneConfiguration asset and try again.", "OK");
            }
        }
    }
}
#endif