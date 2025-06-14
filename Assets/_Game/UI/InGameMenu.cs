using F3PS;
using StarterAssets;
using UnityEngine;
using UnityEngine.Windows;

public class InGameMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject parent;
    public GameObject menuSelection;
    public GameObject settings;
    public GameObject levelSelection;

    [Space(10)]
    [Header("Watchers")]
    public bool isReloading = false;
    public bool wasMenuPressedLastFrame = false;
    public bool isMenuOpen = false;

    private void Update()
    {
        if (SceneLoader.Instance.isLoading) return;

        var isMenuPressedThisFrame = GameManager.Instance.inputs.menu;
        var isKeyDown = !wasMenuPressedLastFrame && isMenuPressedThisFrame;
        wasMenuPressedLastFrame = isMenuPressedThisFrame;
        if (isKeyDown && !isMenuOpen)
        {
            OnShowMainMenu();
            isMenuOpen = true;
            GameManager.Instance.StopGameAfterMenuOpened();
        }
        else if (isKeyDown && isMenuOpen)
        {
            HideAll();
            isMenuOpen = false;
            GameManager.Instance.ResumeGameAfterMenuClosed();
        }
    }

    public void OnResumeGame()
    {
        HideAll();
        isMenuOpen = false;
        GameManager.Instance.ResumeGameAfterMenuClosed();
    }

    public void OnShowMainMenu()
    {
        parent.SetActive(true);
        menuSelection.SetActive(true);
        settings.SetActive(false);
        levelSelection.SetActive(false);
    }

    public void HideAll()
    {
        parent.SetActive(false);
        menuSelection.SetActive(false);
        settings.SetActive(false);
        levelSelection.SetActive(false);
    }

    public void OnShowSettings()
    {
        menuSelection.SetActive(false);
        settings.SetActive(true);
        levelSelection.SetActive(false);
    }

    public void OnShowLevelSelection()
    {
        menuSelection.SetActive(false);
        settings.SetActive(false);
        levelSelection.SetActive(true);
    }

    public void OnRestartLevel()
    {
        SceneLoader.Instance.ReloadScene();
    }
}
