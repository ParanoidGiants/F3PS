using DG.Tweening;
using F3PS;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthHUD : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;

    public Image healthBar;
    public Image healthBarBackground;

    void Start()
    {
        healthBar.fillAmount = 1f;
        PlayerEventController.OnCurrentHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(int currentHealth)
    {
        float healthPercentage = (float)currentHealth / PlayerData.MaxHealth;
        healthBar.fillAmount = healthPercentage;
        healthBarBackground.DOFillAmount(healthPercentage, 0.5f).SetDelay(0.5f);
    }
}
