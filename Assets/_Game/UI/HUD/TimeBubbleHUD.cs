using F3PS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeBubbleHUD : MonoBehaviour
{
    private TimeBubbleSkillData TimeBubbleData => GameManager.Instance.PlayerData.TimeBubbleSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    public GameObject timeBubbleBar;
    public Image lifeTimeCircle;
    public Image icon;
    public Image timeScaleBar;
    public TextMeshProUGUI timeScaleBarText;

    private void Awake()
    {
        PlayerEventController.OnTimeBubbleTimeScaleChanged += UpdateTimeScale;
        PlayerEventController.OnTimeBubbleActiveTimeChanged += UpdateActiveTime;
    }

    public void UpdateActiveTime(float activeTime)
    {
        var percentage = activeTime / TimeBubbleData.ActiveDuration;
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

    public void ShowGrenade()
    {
        icon.gameObject.SetActive(true);
    }

    public void SetGrenadeVisible(bool visible)
    {
        icon.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        timeBubbleBar.SetActive(false);
    }

    private void OnEnable()
    {
        timeBubbleBar.SetActive(true);
    }
}
