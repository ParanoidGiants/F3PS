using F3PS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KhonsuSphereHUD : MonoBehaviour
{
    private KhonsuSphereSkillData KhonsuSphereData => GameManager.Instance.GameData.PlayerData.KhonsuSphereSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.saveGameManager.PlayerEventController;

    public GameObject khonsuSphereBar;
    public Image lifeTimeCircle;
    public Image icon;
    public Image timeScaleBar;
    public TextMeshProUGUI timeScaleBarText;

    private void OnEnable()
    {
        // khonsuSphereBar.SetActive(true);
        // PlayerEventController.OnKhonsuSphereTimeScaleChanged += UpdateTimeScale;
        PlayerEventController.OnKhonsuSphereActiveTimeChanged += UpdateActiveTime;
        PlayerEventController.OnKhonsuSphereCoolDownTimeChanged += UpdateCoolDownTime;
    }
    private void OnDisable()
    {
        // khonsuSphereBar.SetActive(false);
        // PlayerEventController.OnKhonsuSphereTimeScaleChanged -= UpdateTimeScale;
        PlayerEventController.OnKhonsuSphereActiveTimeChanged -= UpdateActiveTime;
        PlayerEventController.OnKhonsuSphereCoolDownTimeChanged -= UpdateCoolDownTime;
    }

    private void UpdateCoolDownTime(float coolDownTime)
    {
        var percentage = coolDownTime / KhonsuSphereData.CoolDownDuration;
        if (percentage == 0f)
        {
            lifeTimeCircle.fillAmount = 0f;
        }
        else
        {
            lifeTimeCircle.fillAmount = 1f - percentage;
        }
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
            lifeTimeCircle.fillAmount = percentage;
        }
    }

    public void UpdateTimeScale(float timeScale)
    {
        timeScaleBar.fillAmount = timeScale;
        timeScaleBarText.text = $"{Mathf.RoundToInt(timeScale * 100)}%";
    }
}
