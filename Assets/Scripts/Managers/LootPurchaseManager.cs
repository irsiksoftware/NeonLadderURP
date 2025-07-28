using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using UnityEngine;

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
                // Schedule currency deduction
                player.ScheduleCurrencyChange(CurrencyType.Meta, -metaItemCost, 0f);
                
                switch (itemName)
                {
                    case "Health Potion":
                        player.ScheduleHealing(10, 0.1f); // Slight delay for visual effect
                        break;
                    case "Stamina Potion":
                        // Schedule stamina increase via negative damage (heals stamina)
                        player.ScheduleStaminaDamage(-10, 0.1f);
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
                // Schedule currency deduction
                player.ScheduleCurrencyChange(CurrencyType.Perma, -permaItemCost, 0f);
                switch (itemName)
                {
                    case "Double Jump":
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
