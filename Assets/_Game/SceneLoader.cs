using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;
    public static SceneLoader Instance => _instance;

    [Header("References")]
    public Image backDrop;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReloadScene()
    {
        backDrop.gameObject.SetActive(true);
        backDrop
            .DOColor(backDrop.color.WithAlpha(1), 0.5f)
            .OnComplete(() => {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        backDrop.gameObject.SetActive(true);
        backDrop
            .DOColor(backDrop.color.WithAlpha(0), 0.5f)
            .OnComplete(() => {
                backDrop.gameObject.SetActive(false);
            });
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
