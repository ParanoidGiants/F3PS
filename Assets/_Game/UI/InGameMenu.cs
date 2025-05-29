using F3PS;
using StarterAssets;
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
        menuSelection.SetActive(true);
        settings.SetActive(false);
        levelSelection.SetActive(false);
    }

    public void ResumeGame()
    {
        FindObjectOfType<ThirdPersonController>().ResumeGame();
    }

    public void CloseMenu()
    {
        parent.SetActive(false);
        menuSelection.SetActive(false);
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
        FindObjectOfType<ThirdPersonController>().ResumeGame();
        SceneLoader.Instance.ReloadScene();
        CloseMenu();
    }
}
