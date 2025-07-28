using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.Utilities
{
    public class ButtonHelper : MonoBehaviour
    {
        private LootPurchaseManager lootPurchaseManager;

        void Start()
        {
            lootPurchaseManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<LootPurchaseManager>();
        }

        public void OnMetaItemButtonClick()
        {
            lootPurchaseManager.PurchaseMetaItem(GetComponentInChildren<TextMeshProUGUI>().text);
        }

        public void OnPermaItemButtonClick()
        {
            var success = lootPurchaseManager.PurchasePermaItem(GetComponentInChildren<TextMeshProUGUI>().text);
            if (success)
            {
                Destroy(gameObject);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (SceneEnumResolver.Resolve(sceneName) == Scenes.Title)
            {
                SaveGameManager.Save(Game.Instance.model.Player);
                Game.Instance.DestroyGameInstance();
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SceneEnumResolver.Resolve(scene.name) == Scenes.Title)
            {
                Time.timeScale = 1f;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
