using DG.Tweening;
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

    public void ReloadScene(float delay = 0f)
    {
        backDrop.gameObject.SetActive(true);
        // Create a new color with full opacity (alpha = 1)
        Color targetColor = new Color(backDrop.color.r, backDrop.color.g, backDrop.color.b, 1f);
        backDrop.DOColor(targetColor, 0.5f)
            .OnComplete(() => {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            })
            .SetDelay(delay);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        backDrop.gameObject.SetActive(true);
        // Create a new color with zero opacity (alpha = 0)
        Color targetColor = new Color(backDrop.color.r, backDrop.color.g, backDrop.color.b, 0f);
        backDrop.DOColor(targetColor, 0.5f)
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
