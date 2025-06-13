using F3PS;
using StarterAssets;
using TMPro;
using UnityEngine;

public class LevelSelection : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI levelName;
    public InGameMenu inGameMenu;
    public void OnSelectLevel()
    {
        GameManager.Instance.ResumeGame();
        GameManager.Instance.inputs.SetCursorLockedState(true);
        FindFirstObjectByType<ThirdPersonController>().ResumeGame();
        SceneLoader.Instance.LoadScene(sceneName);
    }

    public void Init(string sceneName, InGameMenu inGameMenu)
    {
        this.sceneName = sceneName;
        this.inGameMenu = inGameMenu;
        levelName.text = sceneName;
    }
}
