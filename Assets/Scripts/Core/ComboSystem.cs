using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Core
{
    /// <summary>
    /// Manages combo detection, timing windows, and special move execution
    /// Works with the input buffer system to enable fighting game mechanics
    /// </summary>
    [System.Serializable]
    public class ComboSystem
    {
        [SerializeField] private float comboWindowTime = 0.5f; // Time between inputs for combos
        [SerializeField] private float inputBufferTime = 0.2f; // How long inputs stay buffered
        
        // Track last input times per player
        private Dictionary<Player, ComboState> playerComboStates = new Dictionary<Player, ComboState>();
        
        // Define available combos
        private List<ComboDefinition> combos = new List<ComboDefinition>();

        public ComboSystem()
        {
            InitializeCombos();
        }

        private void InitializeCombos()
        {
            // Basic 3-hit combo: Attack -> Attack -> Attack
            combos.Add(new ComboDefinition
            {
                id = "basic_3hit",
                name = "Triple Strike",
                inputs = new[] { InputType.Attack, InputType.Attack, InputType.Attack },
                damageMultipliers = new[] { 1.0f, 1.2f, 1.5f },
                windowTimes = new[] { 0.5f, 0.4f, 0.3f } // Gets tighter
            });

            // Jump cancel combo: Attack -> Jump -> Attack
            combos.Add(new ComboDefinition
            {
                id = "aerial_strike",
                name = "Rising Dragon",
                inputs = new[] { InputType.Attack, InputType.Jump, InputType.Attack },
                damageMultipliers = new[] { 1.0f, 0f, 2.0f },
                requiresGrounded = new[] { true, true, false }
            });

            // Special move: Sprint -> Attack -> Attack
            combos.Add(new ComboDefinition
            {
                id = "dash_combo",
                name = "Lightning Rush",
                inputs = new[] { InputType.Sprint, InputType.Attack, InputType.Attack },
                damageMultipliers = new[] { 0f, 1.5f, 1.8f },
                staminaCosts = new[] { 5f, 10f, 15f }
            });
        }

        public bool IsInComboWindow(Player player)
        {
            if (!playerComboStates.TryGetValue(player, out var state))
                return false;
                
            return Time.time - state.lastInputTime <= comboWindowTime;
        }

        public void CheckComboCompletion(Player player, InputBufferEvent input)
        {
            if (!playerComboStates.TryGetValue(player, out var state))
            {
                state = new ComboState();
                playerComboStates[player] = state;
            }

            // Add input to history
            state.inputHistory.Add(new TimedInput
            {
                type = input.inputType,
                time = Time.time
            });

            // Clean old inputs
            state.inputHistory.RemoveAll(i => Time.time - i.time > comboWindowTime * 3);

            // Check for combo matches
            foreach (var combo in combos)
            {
                if (MatchesCombo(state.inputHistory, combo))
                {
                    ExecuteCombo(player, combo);
                    state.inputHistory.Clear(); // Reset after successful combo
                    break;
                }
            }

            state.lastInputTime = Time.time;
        }

        private bool MatchesCombo(List<TimedInput> history, ComboDefinition combo)
        {
            if (history.Count < combo.inputs.Length)
                return false;

            // Check the last N inputs match the combo
            int startIndex = history.Count - combo.inputs.Length;
            
            for (int i = 0; i < combo.inputs.Length; i++)
            {
                var input = history[startIndex + i];
                
                // Check input type matches
                if (input.type != combo.inputs[i])
                    return false;
                    
                // Check timing window
                if (i > 0)
                {
                    var prevInput = history[startIndex + i - 1];
                    var timeDiff = input.time - prevInput.time;
                    var windowTime = combo.windowTimes?[i - 1] ?? comboWindowTime;
                    
                    if (timeDiff > windowTime)
                        return false;
                }
            }

            return true;
        }

        private void ExecuteCombo(Player player, ComboDefinition combo)
        {
            Debug.Log($"COMBO! {combo.name}");
            
            // Schedule combo execution event
            var comboEvent = Simulation.Schedule<ComboExecutionEvent>(0f);
            comboEvent.player = player;
            comboEvent.combo = combo;
            
            // Visual feedback
            var vfxEvent = Simulation.Schedule<ComboVFXEvent>(0f);
            vfxEvent.position = player.transform.position;
            vfxEvent.comboName = combo.name;
        }

        private class ComboState
        {
            public float lastInputTime;
            public List<TimedInput> inputHistory = new List<TimedInput>();
        }

        private class TimedInput
        {
            public InputType type;
            public float time;
        }
    }

    [System.Serializable]
    public class ComboDefinition
    {
        public string id;
        public string name;
        public InputType[] inputs;
        public float[] damageMultipliers;
        public float[] windowTimes; // Optional per-input timing windows
        public float[] staminaCosts;
        public bool[] requiresGrounded;
    }

    /// <summary>
    /// Event that executes a combo's effects
    /// </summary>
    public class ComboExecutionEvent : Simulation.Event
    {
        public Player player;
        public ComboDefinition combo;

        public override void Execute()
        {
            // Apply combo effects
            Debug.Log($"Executing combo: {combo.name}");
            
            // Could trigger special animations, damage, effects, etc.
            // This is where the combo's actual gameplay effects happen
        }
    }

    /// <summary>
    /// Visual effects for successful combos
    /// </summary>
    public class ComboVFXEvent : Simulation.Event
    {
        public Vector3 position;
        public string comboName;

        public override void Execute()
        {
            // Spawn combo name popup, particle effects, etc.
            Debug.Log($"COMBO VFX: {comboName} at {position}");
        }
    }
}