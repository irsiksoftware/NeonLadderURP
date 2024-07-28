using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class RemoveNumericSuffix : Editor
{
    [MenuItem("Tools/RenameHelper/Remove Numeric Suffix")]
    static void RemoveNumericSuffixFromScene()
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
                Undo.RecordObject(obj, "Remove Numeric Suffix");
                string cleanName = RemoveSuffix(obj.name, prefabName);
                obj.name = cleanName;
            }
        }
    }

    static string RemoveSuffix(string objectName, string baseName)
    {
        string pattern = @"_\d+$";
        return Regex.Replace(objectName, pattern, "");
    }
}
