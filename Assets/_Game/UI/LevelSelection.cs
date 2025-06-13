using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI levelName;
    public void OnSelectLevel()
    {
        SceneLoader.Instance.LoadScene(sceneName);
        FindFirstObjectByType<InGameMenu>().CloseMenu();
    }

    public void Init(string sceneName)
    {
        this.sceneName = sceneName;
        levelName.text = sceneName;
    }
}
