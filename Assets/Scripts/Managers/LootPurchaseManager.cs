using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Managers
{
    public class LootPurchaseManager : MonoBehaviour
    {
        protected PlatformerModel model;
        protected Player player;

        private const int metaItemCost = 10; // Leave this static for now.
        private const int permaItemCost = 10; // Leave this static for now.

        void Start()
        {
            model = Simulation.GetModel<PlatformerModel>();
            player = model.Player;
        }

        private void Awake()
        {
            enabled = false;

        }

        public void PurchaseMetaItem(string itemName)
        {
            if (player == null)
            {
                Debug.LogError($"{nameof(LootPurchaseManager)} -> {nameof(PurchaseMetaItem)} -> Player reference magically disappeared.");
            }

            if (player.MetaCurrency.current >= metaItemCost)
            {
                player.MetaCurrency.Decrement(metaItemCost);
                switch (itemName)
                {
                    case "Health Potion":
                        player.Health.Increment(10);
                        break;
                    case "Stamina Potion":
                        player.Stamina.Increment(10);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.Log("Not enough meta currency to purchase item.");
            }
        }


        public bool PurchasePermaItem(string itemName)
        {
            bool result = false;
            if (player == null)
            {
                Debug.LogError($"{nameof(LootPurchaseManager)} -> {nameof(PurchasePermaItem)} -> Player reference magically disappeared.");
            }

            if (player.PermaCurrency.current >= permaItemCost)
            {
                result = true;
                player.PermaCurrency.Decrement(permaItemCost);
                switch (itemName)
                {
                    case "Extra Jump":
                        player.Actions.IncrementAvailableMidAirJumps();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Debug.Log("Not enough meta currency to purchase item.");
            }
            return result;
        }
    }
}
