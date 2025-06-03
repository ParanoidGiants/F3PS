using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Reference")]
    public GameObject prefab;
    [Header("Settings")]
    public int numberOfPooledObjects = 20;
    private List<GameObject> _objects;
    public void Init(Transform parent)
    {
        _objects = new List<GameObject>();
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            _objects.Add(obj);
        }
    }
    public GameObject GetObject()
    {
        foreach (GameObject obj in _objects)
        {
            if (!obj.gameObject.activeInHierarchy)
            {
                return obj;
            }
        }

        Debug.LogWarning("There are no available objects in pool!");
        return null;
    }
}