using F3PS;
using UnityEngine;

public class InGameMenu : MonoBehaviour
{
    public GameObject parent;
    public GameObject menuSelection;
    public GameObject settings;
    public GameObject levelSelection;
    public bool isReloading = false;

    public void OpenMenu()
    {
        parent.SetActive(true);
        OpenMenuSelection();
    }

    public void CloseMenu()
    {
        parent.SetActive(false);
        menuSelection.SetActive(false);
        settings.SetActive(false);
        levelSelection.SetActive(false);
    }

    public void OpenMenuSelection()
    {
        menuSelection.SetActive(true);
        settings.SetActive(false);
        levelSelection.SetActive(false);
    }

    public void OpenSettings()
    {
        menuSelection.SetActive(false);
        settings.SetActive(true);
        levelSelection.SetActive(false);
    }

    public void OpenLevelSelection()
    {
        menuSelection.SetActive(false);
        settings.SetActive(false);
        levelSelection.SetActive(true);
    }

    public void RestartLevel()
    {
        SceneLoader.Instance.ReloadScene();
        CloseMenu();
    }
}
