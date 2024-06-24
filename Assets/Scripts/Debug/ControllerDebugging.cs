
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class ControllerDebugging
{

    private static string bindingOutput = string.Empty;

    public static void PrintDebugControlConfiguration(Player player)
    {
        if (bindingOutput == string.Empty)
        {
            var actionBindings = new Dictionary<string, List<string>>();

            foreach (var action in player.Controls.FindActionMap("Player").actions)
            {
                foreach (var binding in action.bindings)
                {
                    string deviceName = FormatDeviceName(binding.path.Split('/')[0].Replace("<", "").Replace(">", "")); // Extract and format device name
                    string controlName = binding.path.Split('/').Last();

                    string formattedBinding = $"{controlName} ({deviceName})";

                    if (!actionBindings.ContainsKey(action.name))
                    {
                        actionBindings[action.name] = new List<string>();
                    }
                    actionBindings[action.name].Add(formattedBinding);
                }
            }
            var fullBindings = new List<string>();
            foreach (var actionBinding in actionBindings)
            {
                string actionName = actionBinding.Key;
                string bindings = string.Join(", ", actionBinding.Value);
                fullBindings.Add($"{actionName} = {bindings}");
            }

            bindingOutput = string.Join("\n", fullBindings);
            Debug.Log(bindingOutput);
        }
    }

    public static string GetDeviceDebugText(InputDevice controller)
    {
        string result = string.Empty;
        if (controller == null)
        {
            result = "Virtual Gamepad UI in use or no input controller detected";
        }
        else if (controller is Gamepad gamepad)
        {
            // Check the name of the gamepad to identify the type
            var name = gamepad.name.ToLowerInvariant();
            if (name.Contains("dualshock") || name.Contains("playstation"))
            {
                result = "PlayStation Controller in use";
            }
            else if (name.Contains("xbox"))
            {
                result = "Xbox Controller in use";
            }
            else if (name.Contains("switch"))
            {
                result = "Switch Controller in use";
            }
            else
            {
                // If none of the above, default to this
                result = $"{gamepad.name} in use";
            }
        }
        else if (controller is Keyboard)
        {
            result = "Keyboard in use";
        }
        else if (controller is Mouse)
        {
            result = "Mouse in use";
        }
        else
        {
            // If it's not a gamepad, keyboard, or mouse, you can default to this
            result = $"{controller.displayName} in use";
        }

        return result;
    }

    private static string FormatDeviceName(string deviceName)
    {
        switch (deviceName)
        {
            case "Keyboard":
                return "Keyboard";
            case "XInputController":
                return "Xbox"; // maybe also steam?
            case "SwitchProControllerHID":
                return "Nintendo Switch";
            case "DualShockGamepad":// Add more cases as needed for other devices
                return "Playstation";
            default:
                return deviceName; // Return the original name if not recognized
        }
    }
}
