using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class RecalculateObjectNames : Editor
{
    [MenuItem("NeonLadder/Tools/Rename Helpers/Recalculate Object Names")]
    static void RecalculateNames()
    {
        GameObject selectedPrefab = Selection.activeObject as GameObject;

        if (selectedPrefab == null)
        {
            Debug.LogError("No prefab selected.");
            return;
        }

        string prefabName = selectedPrefab.name;

        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        // Filter out the main camera and sort by X position
        List<GameObject> objectsToRename = allGameObjects
            .Where(obj => obj.name.Contains(prefabName))
            .OrderBy(obj => obj.transform.position.x)
            .ToList();

        // Rename the objects
        for (int i = 0; i < objectsToRename.Count; i++)
        {
            Undo.RecordObject(objectsToRename[i], "Recalculate Object Names");
            objectsToRename[i].name = $"{prefabName}_{i + 1}";
        }

        // Reorder objects in the hierarchy
        ReorderObjectsInHierarchy(objectsToRename);
    }

    static void ReorderObjectsInHierarchy(List<GameObject> objectsToReorder)
    {
        for (int i = 0; i < objectsToReorder.Count; i++)
        {
            Undo.RecordObject(objectsToReorder[i].transform, "Reorder Objects in Hierarchy");
            // Move each object to the end of the hierarchy, which effectively sorts them
            objectsToReorder[i].transform.SetSiblingIndex(i);
        }
    }
}
