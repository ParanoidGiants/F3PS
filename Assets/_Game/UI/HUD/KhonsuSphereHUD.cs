using F3PS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KhonsuSphereHUD : MonoBehaviour
{
    private KhonsuSphereSkillData KhonsuSphereData => GameManager.Instance.PlayerData.KhonsuSphereSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    public GameObject khonsuSphereBar;
    public Image lifeTimeCircle;
    public Image icon;
    public Image timeScaleBar;
    public TextMeshProUGUI timeScaleBarText;

    private void Awake()
    {
        PlayerEventController.OnKhonsuSphereTimeScaleChanged += UpdateTimeScale;
        PlayerEventController.OnKhonsuSphereActiveTimeChanged += UpdateActiveTime;
    }

    public void UpdateActiveTime(float activeTime)
    {
        var percentage = activeTime / KhonsuSphereData.ActiveDuration;
        if (percentage == 0f)
        {
            lifeTimeCircle.fillAmount = 0f;
        }
        else
        {
            lifeTimeCircle.fillAmount = 1f - percentage;
        }
    }

    public void UpdateTimeScale(float timeScale)
    {
        timeScaleBar.fillAmount = timeScale;
        timeScaleBarText.text = $"{Mathf.RoundToInt(timeScale * 100)}%";
    }

    private void OnDisable()
    {
        khonsuSphereBar.SetActive(false);
        Debug.Log("Khonsu Sphere HUD disabled");
    }

    private void OnEnable()
    {
        khonsuSphereBar.SetActive(true);
        Debug.Log("Khonsu Sphere HUD enabled");
    }
}
