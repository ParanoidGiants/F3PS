using F3PS;
using StarterAssets;
using UnityEngine;
using UnityEngine.Windows;

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
        GameManager.Instance.ResumeGame();
        GameManager.Instance.inputs.SetCursorLockedState(true);
        FindFirstObjectByType<ThirdPersonController>().ResumeGame();
        CloseMenu();
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
        FindFirstObjectByType<ThirdPersonController>().ResumeGame();
        SceneLoader.Instance.ReloadScene();
    }
}
