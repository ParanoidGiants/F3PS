using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionGroup : MonoBehaviour
{
    public GameObject LevelSelectionPrefab;
    public List<LevelSelection> levelSelections = new List<LevelSelection>();

    void Start()
    {
        foreach (string sceneName in SceneLoader.Instance.sceneNames)
        {
            var levelSelection = Instantiate(LevelSelectionPrefab, transform).GetComponent<LevelSelection>();
            levelSelection.Init(sceneName);
            levelSelections.Add(levelSelection);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }
}
