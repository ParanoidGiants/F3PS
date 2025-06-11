using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindObjectsWithMissingScripts
{
    [UnityEditor.MenuItem("My Tools/Find Objects with Missing Scripts")]
    public static void FindMissingScripts()
    {
        var objects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
        foreach (var obj in objects)
        {
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogWarning($"GameObject '{obj.name}' has a missing script!", obj);
                }
            }
        }
        Debug.Log("Search completed.");
    }
}
