using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static NeonLadder.Core.Simulation;

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
    private bool isScrolling = true;
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
        // Use SceneTransitionManager for proper spawn handling
        if (SceneTransitionManager.Instance != null)
        {
            Debug.Log($"[AutoScrollText] Using SceneTransitionManager to transition to: {targetScene}");
            SceneTransitionManager.Instance.TransitionToScene(targetScene.ToString());
        }
        else
        {
            Debug.LogWarning($"[AutoScrollText] SceneTransitionManager not found, falling back to direct load of: {targetScene}");
            SceneManager.LoadScene(targetScene.ToString());
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
}
