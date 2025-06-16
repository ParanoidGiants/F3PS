using TMPro;
using UnityEngine;

public class LevelSelection : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI levelName;
    public InGameMenu inGameMenu;
    public void OnSelectLevel()
    {
        SceneLoader.Instance.LoadScene(sceneName);
    }

    public void Init(string sceneName)
    {
        this.sceneName = sceneName;
        levelName.text = sceneName;
    }
}
