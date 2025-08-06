using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// ScriptableObject configuration for dialogue scenes
    /// Create via Assets -> Create -> NeonLadder -> Dialogue Scene Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Scene", menuName = "NeonLadder/Dialogue Scene Configuration", order = 1)]
    public class DialogueSceneConfiguration : ScriptableObject
    {
        [Header("Scene Configuration")]
        [Tooltip("Name of this dialogue scene")]
        public string sceneName = "Dialogue Scene";
        
        [Tooltip("Description for developers")]
        [TextArea(3, 5)]
        public string description = "Drag characters to left/right positions and configure dialogue interactions";

        [Header("Character Setup")]
        [Tooltip("Character placed on the left side")]
        public DialogueCharacterConfig leftCharacter;
        
        [Tooltip("Character placed on the right side")]
        public DialogueCharacterConfig rightCharacter;

        [Header("Dialogue Trigger Setup")]
        [Tooltip("Size of the trigger box for initiating dialogue")]
        public Vector3 triggerBoxSize = new Vector3(2f, 2f, 2f);
        
        [Tooltip("Position offset for the trigger box")]
        public Vector3 triggerBoxOffset = Vector3.zero;
        
        [Tooltip("Layer mask for what can trigger dialogue (usually Player)")]
        public LayerMask triggerLayerMask = 1;

        [Header("Dialogue Configuration")]
        [Tooltip("Type of dialogue interaction")]
        public DialogueInteractionType interactionType = DialogueInteractionType.FullConversation;
        
        [Tooltip("Conversation name in the Dialogue Database")]
        public string conversationName = "";
        
        [Tooltip("Enable automatic setup of Pixel Crushers components")]
        public bool autoSetupPixelCrushersComponents = true;

        [Header("Camera and UI")]
        [Tooltip("Camera position for dialogue scenes")]
        public DialogueCameraSetup cameraSetup;
        
        [Tooltip("UI canvas prefab for dialogue")]
        public GameObject dialogueUIPrefab;

        [Header("Localization")]
        [Tooltip("Supported languages for this dialogue")]
        public string[] supportedLanguages = { "en", "zh-Hans", "ur" };
        
        [Tooltip("Text table name for localization")]
        public string textTableName = "";

        /// <summary>
        /// Generate a complete dialogue scene setup
        /// </summary>
        [ContextMenu("Generate Dialogue Scene")]
        public void GenerateDialogueScene()
        {
            #if UNITY_EDITOR
            NeonLadder.Dialog.Editor.DialogueSceneGenerator.GenerateScene(this);
            #endif
        }

        /// <summary>
        /// Validate the configuration
        /// </summary>
        public bool ValidateConfiguration(out string errorMessage)
        {
            errorMessage = "";

            if (leftCharacter == null)
            {
                errorMessage = "Left character is not assigned";
                return false;
            }

            if (rightCharacter == null)
            {
                errorMessage = "Right character is not assigned";
                return false;
            }

            if (string.IsNullOrEmpty(conversationName))
            {
                errorMessage = "Conversation name is empty";
                return false;
            }

            if (triggerBoxSize.x <= 0 || triggerBoxSize.y <= 0 || triggerBoxSize.z <= 0)
            {
                errorMessage = "Trigger box size must be positive";
                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public class DialogueCharacterConfig
    {
        [Header("Character Identity")]
        public string characterName = "";
        public GameObject characterPrefab;
        
        [Header("Positioning")]
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
        
        [Header("Dialogue Settings")]
        public string actorName = "";
        public DialogueCharacterRole role = DialogueCharacterRole.NPC;
        
        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
        public string idleAnimationName = "Idle";
        public string talkingAnimationName = "Talking";

        [Header("Audio")]
        public AudioClip[] voiceClips;
        public AudioSource audioSource;
    }

    [System.Serializable]
    public class DialogueCameraSetup
    {
        public Vector3 position = new Vector3(0, 1.5f, -3);
        public Vector3 rotation = new Vector3(10, 0, 0);
        public float fieldOfView = 60f;
        public bool useCustomCamera = false;
        public GameObject customCameraPrefab;
    }

    public enum DialogueInteractionType
    {
        SimpleBanter,
        ProtagonistReply,
        PlayerChoice,
        FullConversation
    }

    public enum DialogueCharacterRole
    {
        Player,
        NPC,
        Boss,
        Merchant,
        Neutral
    }
}