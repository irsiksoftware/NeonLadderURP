using UnityEngine;
using UnityEditor;

public class RemoveAppendedCounts : Editor
{
    [MenuItem("Tools/RenameHelper/Remove Appended Counts")]
    static void RemoveAppendedCountsFromScene()
    {
        GameObject selectedPrefab = Selection.activeObject as GameObject;

        if (selectedPrefab == null)
        {
            Debug.LogError("No prefab selected.");
            return;
        }

        string prefabName = selectedPrefab.name;

        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allGameObjects)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == selectedPrefab)
            {
                Undo.RecordObject(obj, "Remove Appended Counts");
                string cleanName = RemoveSuffix(obj.name, prefabName);
                obj.name = cleanName;
            }
        }
    }

    static string RemoveSuffix(string objectName, string baseName)
    {
        int suffixIndex = objectName.IndexOf(" (");
        if (suffixIndex != -1)
        {
            return baseName;
        }

        return objectName;
    }
}
