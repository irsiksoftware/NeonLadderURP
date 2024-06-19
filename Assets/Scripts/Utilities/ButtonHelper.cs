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

        public void OnButtonClick()
        {
            lootPurchaseManager.PurchaseMetaItem(GetComponentInChildren<TextMeshProUGUI>().text);
        }
    }
}
