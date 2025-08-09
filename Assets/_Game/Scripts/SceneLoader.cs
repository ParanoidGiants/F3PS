using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;
    public static SceneLoader Instance => _instance;
    public bool isLoading;

    public string[] sceneNames = new string[]
    {
        "_menu",
        "_controls",
        "_kill_enemy_opens_door"
    };

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
    }

    private void Start()
    {
        isLoading = true;
        backDrop.gameObject.SetActive(true);
        HideBackdrop();
    }

    public void ReloadScene(float delay = 0f)
    {
        if (isLoading) return;

        LoadScene(SceneManager.GetActiveScene().name, delay);
    }

    public void LoadScene(string sceneName, float delay = 0f)
    {
        if (isLoading) return;

        isLoading = true;
        backDrop.gameObject.SetActive(true);
        Color targetColor = new Color(backDrop.color.r, backDrop.color.g, backDrop.color.b, 1f);
        backDrop.DOColor(targetColor, 0.5f)
            .OnComplete(() => SceneManager.LoadScene(sceneName))
            .SetDelay(delay)
            .SetUpdate(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        backDrop.gameObject.SetActive(true);
        HideBackdrop();
    }

    private void HideBackdrop()
    {
        Color initialColor = backDrop.color;
        initialColor.a = 1f;
        Color targetColor = backDrop.color;
        targetColor.a = 0f;

        backDrop.color = initialColor;
        backDrop.DOColor(targetColor, 0.5f)
            .OnComplete(() => {
                backDrop.gameObject.SetActive(false);
                isLoading = false;
            });
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        DOTween.KillAll();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
