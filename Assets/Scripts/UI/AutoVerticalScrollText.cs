using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoScrollText : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform contentRectTransform;
    private TextMeshProUGUI textMeshPro;
    private string text;
    public float scrollSpeed = 20f;
    private bool isScrolling = true;
    private float bufferHeightMultiplier = 0.20f;
    private float lineBreakHeightMultiplier = 10f;
    private float textHeightMultiplier = 0.75f;
    private float initialTextOffset = 1.25f;
    private float bufferHeight;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        MakeScrollbarTransparent(scrollRect.verticalScrollbar);
        scrollRectTransform = GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;
        contentRectTransform = scrollRect.content.GetComponent<RectTransform>();
        contentRectTransform.anchorMin = new Vector2(0, 0);
        contentRectTransform.anchorMax = new Vector2(1, 1);
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        text = textMeshPro.text;
        bufferHeight = (text.Split('\n').Length - 1) * lineBreakHeightMultiplier + text.Length * bufferHeightMultiplier;
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
        Debug.Log("Text is finished scrolling");
    }

    private void SetTextMeshProWidth()
    {
        RectTransform textRectTransform = textMeshPro.GetComponent<RectTransform>();
        RectTransform canvasRectTransform = GetComponent<RectTransform>();
        float width = canvasRectTransform.rect.width * textHeightMultiplier;
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void AdjustContentHeight()
    {
        RectTransform textRectTransform = textMeshPro.GetComponent<RectTransform>();
        float preferredHeight = textMeshPro.preferredHeight;
        float totalHeight = preferredHeight + bufferHeight;
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }

    private void MakeScrollbarTransparent(Scrollbar scrollbar)
    {
        Color transparent = new Color(0, 0, 0, 0);
        scrollbar.GetComponent<Image>().color = transparent;
        scrollbar.targetGraphic.color = transparent;
        scrollbar.handleRect.GetComponent<Image>().color = transparent;
    }
}
