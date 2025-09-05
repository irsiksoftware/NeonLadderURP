using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using NeonLadder.UI.Shop;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Editor helper to create shop UI prefabs automatically
    /// </summary>
    public class ShopUICreator : EditorWindow
    {
        [MenuItem("NeonLadder/UI/Create Shop UI Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<ShopUICreator>("Shop UI Creator");
        }
        
        [MenuItem("NeonLadder/UI/Create Basic Shop Window")]
        public static void CreateBasicShopWindow()
        {
            // Create the main canvas
            GameObject canvasObj = new GameObject("ShopCanvas");
            canvasObj.tag = "ShopkeeperWindow"; // Set the tag
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Above game UI
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Add ShopWindowUI component
            ShopWindowUI shopUI = canvasObj.AddComponent<ShopWindowUI>();
            
            // Create background
            GameObject bg = CreateUIGameObject("Background", canvasObj.transform);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Semi-transparent dark
            SetFullScreen(bg.GetComponent<RectTransform>());
            
            // Create close button
            GameObject closeBtn = CreateButton("CloseButton", canvasObj.transform, "✕");
            RectTransform closeBtnRT = closeBtn.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 1);
            closeBtnRT.anchorMax = new Vector2(1, 1);
            closeBtnRT.anchoredPosition = new Vector2(-30, -30);
            closeBtnRT.sizeDelta = new Vector2(50, 50);
            
            // Create shop name
            GameObject shopName = CreateText("ShopName", canvasObj.transform, "Shop Name");
            RectTransform shopNameRT = shopName.GetComponent<RectTransform>();
            shopNameRT.anchorMin = new Vector2(0.5f, 1);
            shopNameRT.anchorMax = new Vector2(0.5f, 1);
            shopNameRT.anchoredPosition = new Vector2(0, -50);
            shopNameRT.sizeDelta = new Vector2(300, 40);
            shopName.GetComponent<TextMeshProUGUI>().fontSize = 24;
            shopName.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            
            // Create currency display
            GameObject currencyPanel = CreateUIGameObject("CurrencyPanel", canvasObj.transform);
            RectTransform currencyRT = currencyPanel.GetComponent<RectTransform>();
            currencyRT.anchorMin = new Vector2(0, 1);
            currencyRT.anchorMax = new Vector2(0, 1);
            currencyRT.anchoredPosition = new Vector2(20, -20);
            currencyRT.sizeDelta = new Vector2(200, 60);
            
            GameObject metaCurrency = CreateText("MetaCurrency", currencyPanel.transform, "Ⓜ 0");
            SetTopLeft(metaCurrency.GetComponent<RectTransform>(), new Vector2(200, 30));
            
            GameObject permaCurrency = CreateText("PermaCurrency", currencyPanel.transform, "Ⓟ 0");
            RectTransform permaRT = permaCurrency.GetComponent<RectTransform>();
            permaRT.anchorMin = new Vector2(0, 0);
            permaRT.anchorMax = new Vector2(1, 0);
            permaRT.anchoredPosition = new Vector2(0, 15);
            permaRT.sizeDelta = new Vector2(200, 30);
            
            // Create scroll view for items
            GameObject scrollView = CreateScrollView("ItemScrollView", canvasObj.transform);
            RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.1f, 0.1f);
            scrollRT.anchorMax = new Vector2(0.9f, 0.8f);
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;
            
            // Get the content object (child of viewport)
            Transform content = scrollView.transform.Find("Viewport/Content");
            
            // Assign references to ShopWindowUI
            SetShopUIReferences(shopUI, shopName, metaCurrency, permaCurrency, 
                               closeBtn, scrollView.GetComponent<ScrollRect>(), content);
            
            // Create prefab
            CreatePrefabFromGameObject(canvasObj, "ShopWindow");
            
            Debug.Log("Basic Shop Window created! Check the Prefabs folder.");
        }
        
        [MenuItem("NeonLadder/UI/Create Shop Item Slot Prefab")]
        public static void CreateShopItemSlot()
        {
            // Create main slot object
            GameObject slot = CreateUIGameObject("ShopItemSlot");
            slot.AddComponent<ShopItemUI>();
            
            // Set size
            RectTransform slotRT = slot.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(400, 80);
            
            // Add background
            GameObject bg = CreateUIGameObject("Background", slot.transform);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            SetFullScreen(bg.GetComponent<RectTransform>());
            
            // Add rarity border
            GameObject border = CreateUIGameObject("RarityBorder", slot.transform);
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = Color.white;
            RectTransform borderRT = border.GetComponent<RectTransform>();
            SetFullScreen(borderRT);
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            
            // Add item icon
            GameObject icon = CreateUIGameObject("ItemIcon", slot.transform);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.preserveAspect = true;
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(50, 0);
            iconRT.sizeDelta = new Vector2(60, 60);
            
            // Add item name
            GameObject itemName = CreateText("ItemName", slot.transform, "Item Name");
            RectTransform nameRT = itemName.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.2f, 0.6f);
            nameRT.anchorMax = new Vector2(0.7f, 1f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            itemName.GetComponent<TextMeshProUGUI>().fontSize = 16;
            
            // Add price text
            GameObject price = CreateText("PriceText", slot.transform, "100 Ⓜ");
            RectTransform priceRT = price.GetComponent<RectTransform>();
            priceRT.anchorMin = new Vector2(0.7f, 0.6f);
            priceRT.anchorMax = new Vector2(1f, 1f);
            priceRT.offsetMin = Vector2.zero;
            priceRT.offsetMax = Vector2.zero;
            price.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            
            // Add description
            GameObject desc = CreateText("ItemDescription", slot.transform, "Item description...");
            RectTransform descRT = desc.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.2f, 0f);
            descRT.anchorMax = new Vector2(0.7f, 0.6f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;
            desc.GetComponent<TextMeshProUGUI>().fontSize = 12;
            desc.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
            
            // Add purchase button
            GameObject buyBtn = CreateButton("PurchaseButton", slot.transform, "Buy");
            RectTransform buyRT = buyBtn.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.75f, 0.1f);
            buyRT.anchorMax = new Vector2(0.95f, 0.5f);
            buyRT.offsetMin = Vector2.zero;
            buyRT.offsetMax = Vector2.zero;
            
            // Assign references
            ShopItemUI itemUI = slot.GetComponent<ShopItemUI>();
            SetShopItemUIReferences(itemUI, iconImage, itemName.GetComponent<TextMeshProUGUI>(),
                                  desc.GetComponent<TextMeshProUGUI>(), price.GetComponent<TextMeshProUGUI>(),
                                  buyBtn.GetComponent<Button>(), bgImage, borderImage);
            
            // Create prefab
            CreatePrefabFromGameObject(slot, "ShopItemSlot");
            
            Debug.Log("Shop Item Slot prefab created! Check the Prefabs folder.");
        }
        
        // Helper methods
        private static GameObject CreateUIGameObject(string name, Transform parent = null)
        {
            GameObject obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            if (parent != null)
                obj.transform.SetParent(parent);
            return obj;
        }
        
        private static GameObject CreateText(string name, Transform parent, string text)
        {
            GameObject textObj = CreateUIGameObject(name, parent);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.color = Color.white;
            return textObj;
        }
        
        private static GameObject CreateButton(string name, Transform parent, string text)
        {
            GameObject btnObj = CreateUIGameObject(name, parent);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.3f);
            Button btn = btnObj.AddComponent<Button>();
            
            // Add text child
            GameObject btnText = CreateText("Text", btnObj.transform, text);
            SetFullScreen(btnText.GetComponent<RectTransform>());
            btnText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            
            return btnObj;
        }
        
        private static GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollObj = CreateUIGameObject(name, parent);
            
            // Add ScrollRect
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scrollObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            
            // Create Viewport
            GameObject viewport = CreateUIGameObject("Viewport", scrollObj.transform);
            viewport.AddComponent<Image>().color = Color.clear;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            SetFullScreen(viewport.GetComponent<RectTransform>());
            
            // Create Content
            GameObject content = CreateUIGameObject("Content", viewport.transform);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);
            
            // Add layout group
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Assign to ScrollRect
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRT;
            scroll.horizontal = false;
            scroll.vertical = true;
            
            return scrollObj;
        }
        
        private static void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        
        private static void SetTopLeft(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }
        
        private static void SetShopUIReferences(ShopWindowUI shopUI, GameObject shopName, 
            GameObject metaCurrency, GameObject permaCurrency, GameObject closeBtn, 
            ScrollRect scrollRect, Transform itemContainer)
        {
            // Use reflection to set private fields (for editor script only)
            var shopUIType = typeof(ShopWindowUI);
            
            shopUIType.GetField("shopNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, shopName.GetComponent<TextMeshProUGUI>());
            
            shopUIType.GetField("metaCurrencyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, metaCurrency.GetComponent<TextMeshProUGUI>());
            
            shopUIType.GetField("permaCurrencyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, permaCurrency.GetComponent<TextMeshProUGUI>());
            
            shopUIType.GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, closeBtn.GetComponent<Button>());
            
            shopUIType.GetField("scrollRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, scrollRect);
            
            shopUIType.GetField("itemContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(shopUI, itemContainer);
        }
        
        private static void SetShopItemUIReferences(ShopItemUI itemUI, Image icon, TextMeshProUGUI itemName,
            TextMeshProUGUI desc, TextMeshProUGUI price, Button buyBtn, Image bg, Image border)
        {
            var itemUIType = typeof(ShopItemUI);
            
            itemUIType.GetField("itemIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, icon);
            
            itemUIType.GetField("itemNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, itemName);
            
            itemUIType.GetField("itemDescriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, desc);
            
            itemUIType.GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, price);
            
            itemUIType.GetField("purchaseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, buyBtn);
            
            itemUIType.GetField("backgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, bg);
            
            itemUIType.GetField("rarityBorder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     ?.SetValue(itemUI, border);
        }
        
        private static void CreatePrefabFromGameObject(GameObject obj, string prefabName)
        {
            // Create Prefabs folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }
            
            string path = $"Assets/Prefabs/UI/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj); // Remove from scene
        }
    }
}