using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Debugging;

namespace NeonLadder.Events
{
    /// <summary>
    /// High-performance batch currency collection event
    /// Collects multiple currency pickups in a single frame for Vampire Survivors-style performance
    /// </summary>
    public class BatchCurrencyEvent : Simulation.Event
    {
        public Player player;
        public List<CurrencyDrop> currencyDrops = new List<CurrencyDrop>();
        
        private static BatchCurrencyEvent _pendingEvent;
        private static float _batchWindow = 0.1f; // 100ms batch window
        
        public override bool Precondition()
        {
            return player != null && currencyDrops.Count > 0;
        }
        
        public override void Execute()
        {
            if (player == null) return;
            
            int totalMeta = 0;
            int totalPerma = 0;
            Vector3 averagePosition = Vector3.zero;
            
            // Batch all currency drops
            foreach (var drop in currencyDrops)
            {
                switch (drop.currencyType)
                {
                    case CurrencyType.Meta:
                        totalMeta += drop.amount;
                        break;
                    case CurrencyType.Perma:
                        totalPerma += drop.amount;
                        break;
                }
                averagePosition += drop.worldPosition;
            }
            
            averagePosition /= currencyDrops.Count;
            
            // Apply currency changes
            if (totalMeta > 0)
            {
                player.MetaCurrency.Increment(totalMeta);
                Debugger.Log($"Collected {totalMeta} Meta currency from {currencyDrops.Count} sources");
            }
            
            if (totalPerma > 0)
            {
                player.PermaCurrency.Increment(totalPerma);
                Debugger.Log($"Collected {totalPerma} Perma currency from {currencyDrops.Count} sources");
            }
            
            // Schedule batch VFX event for visual feedback
            ScheduleBatchVFX(averagePosition, totalMeta, totalPerma);
            
            // Reset pending event
            _pendingEvent = null;
        }
        
        private void ScheduleBatchVFX(Vector3 position, int metaAmount, int permaAmount)
        {
            // Could schedule a VFX event here to show currency collection
            // This would be much more efficient than individual VFX per pickup
            Debugger.Log($"Showing batch currency VFX at {position}: {metaAmount} Meta, {permaAmount} Perma");
        }
        
        /// <summary>
        /// Static method to efficiently batch currency collection
        /// Call this from mob death, currency pickups, etc.
        /// </summary>
        public static void CollectCurrency(Player player, CurrencyType currencyType, int amount, Vector3 worldPosition)
        {
            if (player == null) return;
            
            // If no pending batch event, create one
            if (_pendingEvent == null)
            {
                _pendingEvent = Simulation.Schedule<BatchCurrencyEvent>(_batchWindow);
                _pendingEvent.player = player;
            }
            
            // Add to the pending batch
            _pendingEvent.currencyDrops.Add(new CurrencyDrop
            {
                currencyType = currencyType,
                amount = amount,
                worldPosition = worldPosition
            });
        }
        
        /// <summary>
        /// Alternative for immediate single currency collection (bypasses batching)
        /// Use when you need immediate currency changes (like purchases)
        /// </summary>
        public static void CollectCurrencyImmediate(Player player, CurrencyType currencyType, int amount)
        {
            if (player == null) return;
            
            var immediateEvent = Simulation.Schedule<CurrencyChangeEvent>(0f);
            immediateEvent.player = player;
            immediateEvent.currencyType = currencyType;
            immediateEvent.amount = amount;
        }
        
        internal override void Cleanup()
        {
            player = null;
            currencyDrops.Clear();
            if (_pendingEvent == this)
            {
                _pendingEvent = null;
            }
        }
    }
    
    /// <summary>
    /// Represents a single currency drop for batching
    /// </summary>
    [System.Serializable]
    public struct CurrencyDrop
    {
        public CurrencyType currencyType;
        public int amount;
        public Vector3 worldPosition; // For VFX positioning
    }
    
    /// <summary>
    /// High-performance currency collector component
    /// Attach to currency pickups, enemies, etc.
    /// </summary>
    public class CurrencyCollector : MonoBehaviour
    {
        [Header("Currency Drop Configuration")]
        [SerializeField] private CurrencyType currencyType = CurrencyType.Meta;
        [SerializeField] private int minAmount = 1;
        [SerializeField] private int maxAmount = 3;
        [SerializeField] private float dropChance = 1f; // 0-1 probability
        
        [Header("Visual")]
        [SerializeField] private GameObject dropVFX;
        [SerializeField] private AudioClip dropSound;
        
        /// <summary>
        /// Call this when enemy dies, treasure chest opens, etc.
        /// </summary>
        public void DropCurrency()
        {
            if (Random.value > dropChance) return;
            
            var player = FindObjectOfType<Player>();
            if (player == null) return;
            
            int amount = Random.Range(minAmount, maxAmount + 1);
            
            // Use batch collection for performance
            BatchCurrencyEvent.CollectCurrency(player, currencyType, amount, transform.position);
            
            // Optional: spawn pickup VFX
            if (dropVFX != null)
            {
                Instantiate(dropVFX, transform.position, Quaternion.identity);
            }
        }
        
        /// <summary>
        /// Call this for guaranteed currency drops (like purchases, rewards)
        /// </summary>
        public void DropCurrencyGuaranteed(int amount)
        {
            var player = FindObjectOfType<Player>();
            if (player == null) return;
            
            BatchCurrencyEvent.CollectCurrency(player, currencyType, amount, transform.position);
        }
        
        /// <summary>
        /// Multiple currency drops at once (boss death, treasure room, etc.)
        /// </summary>
        public void DropMultipleCurrency(int count)
        {
            var player = FindObjectOfType<Player>();
            if (player == null) return;
            
            for (int i = 0; i < count; i++)
            {
                if (Random.value <= dropChance)
                {
                    int amount = Random.Range(minAmount, maxAmount + 1);
                    Vector3 position = transform.position + Random.insideUnitSphere * 2f;
                    
                    BatchCurrencyEvent.CollectCurrency(player, currencyType, amount, position);
                }
            }
        }
    }
}