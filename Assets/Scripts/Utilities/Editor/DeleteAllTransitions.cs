using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class DeleteAllTransitions : ScriptableObject
{
    [MenuItem("NeonLadder/Tools/Animation/Delete All Transitions")]
    static void DeleteAllTransitionsFromSelectedController()
    {
        Object selectedObject = Selection.activeObject;

        if (selectedObject is AnimatorController)
        {
            AnimatorController animatorController = (AnimatorController)selectedObject;

            // Register the current state of the animatorController for undo
            Undo.RegisterCompleteObjectUndo(animatorController, "Delete All Transitions");

            foreach (AnimatorControllerLayer layer in animatorController.layers)
            {
                // Remove transitions from states
                foreach (var state in layer.stateMachine.states)
                {
                    var stateTransitions = state.state.transitions;
                    for (int i = stateTransitions.Length - 1; i >= 0; i--)
                    {
                        Undo.RecordObject(state.state, "Delete Transition");
                        state.state.RemoveTransition(stateTransitions[i]);
                    }
                }

                // Remove any state transitions
                var anyStateTransitions = layer.stateMachine.anyStateTransitions;
                for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    Undo.RecordObject(layer.stateMachine, "Delete Any State Transition");
                    layer.stateMachine.RemoveAnyStateTransition(anyStateTransitions[i]);
                }

                // Remove entry transitions
                var entryTransitions = layer.stateMachine.entryTransitions;
                for (int i = entryTransitions.Length - 1; i >= 0; i--)
                {
                    Undo.RecordObject(layer.stateMachine, "Delete Entry Transition");
                    layer.stateMachine.RemoveEntryTransition(entryTransitions[i]);
                }
            }

            // Mark the animatorController as dirty to ensure the changes are saved
            EditorUtility.SetDirty(animatorController);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogWarning("Please select an AnimatorController in the Project window.");
        }
    }
}
