using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;
using TMPro;
using UnityEngine;

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
    }
}
