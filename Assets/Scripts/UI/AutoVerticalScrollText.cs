using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-scrolls the content of a ScrollRect vertically, like the star wars intro text.
/// </summary>
public class AutoScrollText : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform contentRectTransform;
    public float scrollSpeed = 20f;
    private bool isScrolling = true; // To keep track of whether the text is still scrolling
    private float bufferHeightMultiplier = 0.10f; // Adjust the multiplier as needed
    private float lineBreakHeightMultiplier = 10f; // Adjust the multiplier as needed
    private float textHeightMultiplier = 0.75f; // Adjust the multiplier as needed

    void Start()
    {
        // Get the ScrollRect component
        scrollRect = GetComponent<ScrollRect>();

        // Make the vertical scrollbar transparent if it exists
        if (scrollRect.verticalScrollbar != null)
        {
            MakeScrollbarTransparent(scrollRect.verticalScrollbar);
        }

        // Get the RectTransform component of the Scroll View
        scrollRectTransform = GetComponent<RectTransform>();

        // Set the anchors to stretch to the full size of the parent (e.g., Canvas)
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);

        // Reset the offsets to fit the parent
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        // Get the RectTransform component of the Content GameObject
        contentRectTransform = scrollRect.content.GetComponent<RectTransform>();

        // Set the anchors to stretch to the full size of the Scroll View
        contentRectTransform.anchorMin = new Vector2(0, 0);
        contentRectTransform.anchorMax = new Vector2(1, 1);

        // Reset the offsets
        contentRectTransform.offsetMin = Vector2.zero;
        contentRectTransform.offsetMax = Vector2.zero;

        // Set TextMeshPro width to 75% of the viewport
        SetTextMeshProWidth();

        // Adjust the height of the content to ensure scrolling
        AdjustContentHeight();

        // Set initial vertical position to the top
        scrollRect.verticalNormalizedPosition = 1;

        // Log initial positions
        Debug.Log($"Initial VerticalNormalizedPosition: {scrollRect.verticalNormalizedPosition}\n" +
                  $"Content Height: {contentRectTransform.rect.height}\n" +
                  $"Scroll Rect Height: {scrollRectTransform.rect.height}\n" +
                  $"Scroll Speed: {scrollSpeed}");
    }

    void Update()
    {
        if (isScrolling)
        {
            Debug.Log($"VerticalNormalizedPosition: {scrollRect.verticalNormalizedPosition} @ Time {Time.time}\n" +
                      $"Content Height: {contentRectTransform.rect.height}\n" +
                      $"Scroll Rect Height: {scrollRectTransform.rect.height}");

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
        Debug.Log("Text is finished scrolling");
        // Here you can add additional code to handle what happens after the scrolling finishes, such as a scene transition.
    }

    private void SetTextMeshProWidth()
    {
        // Find the TextMeshProUGUI component within the children
        TextMeshProUGUI textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            RectTransform textRectTransform = textMeshPro.GetComponent<RectTransform>();
            RectTransform canvasRectTransform = GetComponent<RectTransform>();

            float width = canvasRectTransform.rect.width * textHeightMultiplier;
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            Debug.Log($"Canvas Width: {canvasRectTransform.rect.width}\n" +
                      $"Text Width: {textRectTransform.rect.width}\n" +
                      $"Text: {textMeshPro.text}");
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found in children.");
        }
    }

    private void AdjustContentHeight()
    {
        // Find the TextMeshProUGUI component within the children
        TextMeshProUGUI textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            RectTransform textRectTransform = textMeshPro.GetComponent<RectTransform>();

            // Calculate the desired height for the content based on the text's preferred height
            float preferredHeight = textMeshPro.preferredHeight;

            // Calculate buffer height based on the text content
            float bufferHeight = CalculateBufferHeight(textMeshPro);

            float totalHeight = preferredHeight + bufferHeight;

            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            // Set the height of the content RectTransform
            contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            Debug.Log($"Adjusted Content Height: {contentRectTransform.rect.height}");
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found in children.");
        }
    }

    private float CalculateBufferHeight(TextMeshProUGUI textMeshPro)
    {
        string text = textMeshPro.text;

        // Count the number of line breaks
        int lineBreakCount = text.Split('\n').Length - 1;

        // Calculate buffer height based on the number of line breaks and total characters
        float bufferHeight = lineBreakCount * lineBreakHeightMultiplier; // Adjust the multiplier as needed
        bufferHeight += text.Length * bufferHeightMultiplier; // Adjust the multiplier as needed

        Debug.Log($"Calculated Buffer Height: {bufferHeight}");
        return bufferHeight;
    }

    private void MakeScrollbarTransparent(Scrollbar scrollbar)
    {
        Color transparent = new Color(0, 0, 0, 0);

        // Set the colors of the scrollbar to transparent
        scrollbar.GetComponent<Image>().color = transparent;
        if (scrollbar.targetGraphic != null)
        {
            scrollbar.targetGraphic.color = transparent;
        }

        // Set the colors of the handle to transparent
        if (scrollbar.handleRect != null)
        {
            Image handleImage = scrollbar.handleRect.GetComponent<Image>();
            if (handleImage != null)
            {
                handleImage.color = transparent;
            }
        }
    }
}
