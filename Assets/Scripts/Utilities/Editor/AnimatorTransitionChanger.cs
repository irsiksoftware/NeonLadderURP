using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorTransitionChanger : MonoBehaviour
{
    [MenuItem("NeonLadder/Tools/Animation/Change Animator Transitions")]
    public static void ChangeAnimatorTransitions()
    {
        // Get the selected object in the Editor
        var selectedObject = Selection.activeObject as AnimatorController;
        if (selectedObject == null)
        {
            Debug.LogError("Please select an AnimatorController in the Project window.");
            return;
        }

        // Iterate through each layer of the AnimatorController
        foreach (var layer in selectedObject.layers)
        {
            var stateMachine = layer.stateMachine;
            ModifyStateMachineTransitions(stateMachine);
        }

        Debug.Log("Animator transitions updated successfully.");
    }

    private static void ModifyStateMachineTransitions(AnimatorStateMachine stateMachine)
    {
        // Iterate through each state in the state machine
        foreach (var state in stateMachine.states)
        {
            ModifyStateTransitions(state.state);
        }

        // Iterate through each sub-state machine
        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            ModifyStateMachineTransitions(subStateMachine.stateMachine);
        }
    }

    private static void ModifyStateTransitions(AnimatorState state)
    {
        // Iterate through each transition from the state
        foreach (var transition in state.transitions)
        {
            transition.hasExitTime = false;
            transition.exitTime = 0f;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
        }
    }
}
