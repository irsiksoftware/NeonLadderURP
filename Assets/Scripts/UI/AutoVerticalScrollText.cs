using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Core;
using NeonLadder.Managers;
using NeonLadder.DataManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeonLadderURP.DataManagement;
using NeonLadder.Mechanics.Controllers;

public class AutoScrollText : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform contentRectTransform;
    private TextMeshProUGUI textMeshPro;
    private RectTransform textRectTransform;
    private RectTransform canvasRectTransform;
    private Image scrollbarImage;
    private Image targetGraphicImage;
    private Image handleImage;
    private string text;
    public float scrollSpeed = 100f;
    private bool isScrolling = false; // Wait for scene to be fully loaded
    public float bufferHeightMultiplier = 0.15f;
    public float lineBreakHeightMultiplier = 10f;
    public float textHeightMultiplier = 0.75f;
    public float initialTextOffset = 1.35f;
    private float bufferHeight;
    public string targetScene;


    void Awake()
    {
        #if UNITY_EDITOR
        scrollSpeed = 200f;
        #endif

        scrollRect = GetComponent<ScrollRect>();
        scrollRectTransform = GetComponent<RectTransform>();
        contentRectTransform = scrollRect.content.GetComponent<RectTransform>();
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        textRectTransform = textMeshPro.GetComponent<RectTransform>();
        canvasRectTransform = GetComponent<RectTransform>();
        scrollbarImage = scrollRect.verticalScrollbar.GetComponent<Image>();
        targetGraphicImage = scrollRect.verticalScrollbar.targetGraphic as Image;
        handleImage = scrollRect.verticalScrollbar.handleRect.GetComponent<Image>();

        SetRectTransformAnchors(scrollRectTransform);
        SetRectTransformAnchors(contentRectTransform);
        MakeScrollbarTransparent();
        text = textMeshPro.text;
        bufferHeight = (text.Split('\n').Length - 1) * lineBreakHeightMultiplier + text.Length * bufferHeightMultiplier;

        // Scene change handling is now done by SceneTransitionManager
    }

    void Start()
    {
        SetTextMeshProWidth();
        AdjustContentHeight();
        scrollRect.verticalNormalizedPosition = initialTextOffset;

        // Subscribe to scene transition completion event
        SceneTransitionManager.OnTransitionCompleted += OnSceneTransitionCompleted;
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        SceneTransitionManager.OnTransitionCompleted -= OnSceneTransitionCompleted;
    }

    private void OnSceneTransitionCompleted(SceneTransitionManager.TransitionData transitionData)
    {
        // Start scrolling if we transitioned TO this scene (BossDefeated or Credits)
        if (transitionData.TargetSceneName == "BossDefeated" || transitionData.TargetSceneName == "Credits")
        {
            Debug.Log($"[AutoScrollText] Starting auto-scroll for scene: {transitionData.TargetSceneName}");
            StartScrolling();
        }
    }

    /// <summary>
    /// Starts the scrolling animation - called after scene load is complete
    /// </summary>
    public void StartScrolling()
    {
        isScrolling = true;
    }

    void Update()
    {
        if (isScrolling)
        {
            if (scrollRect.verticalNormalizedPosition > 0)
            {
                scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime / contentRectTransform.rect.height;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 0;
                isScrolling = false;
                OnScrollFinished();
            }
        }
    }

    private void OnScrollFinished()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[AutoScrollText] Scroll finished in scene: {currentSceneName}");

        // Determine target scene based on current scene
        string nextScene = targetScene;
        if (currentSceneName == "Credits")
        {
            nextScene = Scenes.Core.Title;
            Debug.Log($"[AutoScrollText] Credits scroll complete - preparing for Title screen transition");

            // Save the game before resetting to Title
            SaveGameBeforeReset();

            // Don't reset singletons yet - do it after transition starts
        }
        else if (currentSceneName == "BossDefeated")
        {
            nextScene = targetScene; // Use configured target scene (typically Staging)
            Debug.Log($"[AutoScrollText] BossDefeated scroll complete - transitioning to: {nextScene}");
        }

        // For Credits to Title transition, destroy everything first, then load scene
        if (currentSceneName == "Credits" && nextScene == Scenes.Core.Title)
        {
            Debug.Log("[AutoScrollText] Credits complete - destroying everything before Title transition");
            DestroyEverythingThenLoadTitle(nextScene);
        }
        else
        {
            // Use SceneTransitionManager for other transitions
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(nextScene, SpawnPointType.Auto);
                Debug.Log($"[AutoScrollText] Scene transition to '{nextScene}' initiated via SceneTransitionManager");
            }
            else
            {
                Debug.LogWarning("[AutoScrollText] SceneTransitionManager.Instance is null - using fallback SceneManager.LoadScene");
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    private void SetTextMeshProWidth()
    {
        float width = canvasRectTransform.rect.width * textHeightMultiplier;
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void AdjustContentHeight()
    {
        float preferredHeight = textMeshPro.preferredHeight;
        float totalHeight = preferredHeight + bufferHeight;
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }

    private void MakeScrollbarTransparent()
    {
        Color transparent = new Color(0, 0, 0, 0);
        scrollbarImage.color = transparent;
        targetGraphicImage.color = transparent;
        handleImage.color = transparent;
    }

    private void SetRectTransformAnchors(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void SaveGameBeforeReset()
    {
        try
        {
            Debug.Log("[AutoScrollText] Saving game state before demo completion reset");

            // Use the same save system as SceneTransitionManager
            var saveData = EnhancedSaveSystem.Load() ?? new ConsolidatedSaveData();

            // Update scene information
            saveData.worldState.currentSceneName = SceneManager.GetActiveScene().name;

            // Save the consolidated data
            EnhancedSaveSystem.Save(saveData);
            Debug.Log("[AutoScrollText] Demo completion save performed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoScrollText] Failed to save game: {e.Message}");
        }
    }

    private void DestroyEverythingThenLoadTitle(string titleSceneName)
    {
        Debug.Log("[AutoScrollText] === STARTING COMPLETE SCENE CLEANUP ===");

        try
        {
            // 1. Destroy ALL objects in current Credits scene
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var sceneObjects = currentScene.GetRootGameObjects();
            Debug.Log($"[AutoScrollText] Destroying {sceneObjects.Length} objects in Credits scene");

            foreach (var obj in sceneObjects)
            {
                if (obj != null)
                {
                    Debug.Log($"[AutoScrollText] Destroying Credits object: {obj.name}");
                    DestroyImmediate(obj);
                }
            }

            Debug.Log("[AutoScrollText] Credits scene objects destroyed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoScrollText] Failed to destroy Credits scene objects: {e.Message}");
        }

        try
        {
            // 2. Destroy ALL DontDestroyOnLoad objects (singletons)
            var dontDestroyScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("DontDestroyOnLoad");
            if (dontDestroyScene.IsValid())
            {
                var persistentObjects = dontDestroyScene.GetRootGameObjects();
                Debug.Log($"[AutoScrollText] Destroying {persistentObjects.Length} DontDestroyOnLoad objects");

                foreach (var obj in persistentObjects)
                {
                    if (obj != null)
                    {
                        Debug.Log($"[AutoScrollText] Destroying DontDestroyOnLoad object: {obj.name}");
                        DestroyImmediate(obj);
                    }
                }
            }

            Debug.Log("[AutoScrollText] DontDestroyOnLoad objects destroyed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoScrollText] Failed to destroy DontDestroyOnLoad objects: {e.Message}");
        }

        try
        {
            // 3. Direct singleton destruction as backup
            if (Game.Instance != null)
            {
                Debug.Log("[AutoScrollText] Force destroying Game singleton");
                DestroyImmediate(Game.Instance.gameObject);
            }

            if (ManagerController.Instance != null)
            {
                Debug.Log("[AutoScrollText] Force destroying ManagerController singleton");
                DestroyImmediate(ManagerController.Instance.gameObject);
            }

            Debug.Log("[AutoScrollText] === ALL OBJECTS DESTROYED - LOADING TITLE SCENE ===");

            // 4. NOW load the Title scene with everything clean
            Debug.Log($"[AutoScrollText] Loading {titleSceneName} with completely clean state");
            SceneManager.LoadScene(titleSceneName);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoScrollText] Failed during final cleanup: {e.Message}");
            // Fallback - still try to load title scene
            SceneManager.LoadScene(titleSceneName);
        }
    }

    private void ResetSingletons()
    {
        try
        {
            Debug.Log("[AutoScrollText] Resetting singletons for fresh Title screen state");

            // Destroy Game singleton (will be recreated in Title scene)
            if (Game.Instance != null)
            {
                Debug.Log("[AutoScrollText] Destroying Game singleton");
                Destroy(Game.Instance.gameObject);
            }

            // Destroy Managers singleton (will be recreated in Title scene)
            if (ManagerController.Instance != null)
            {
                Debug.Log("[AutoScrollText] Destroying ManagerController singleton");
                Destroy(ManagerController.Instance.gameObject);
            }

            Debug.Log("[AutoScrollText] Singleton reset complete - fresh Title screen state ready");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AutoScrollText] Failed to reset singletons: {e.Message}");
        }
    }
}
