using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthBar;
    public Image healthBarBackground;

    void Start()
    {
        healthBar.fillAmount = 1f;
    }

    public void UpdateHealth(float healthPercentage)
    {
        healthBar.fillAmount = healthPercentage;
        healthBarBackground.DOFillAmount(healthPercentage, 0.5f).SetDelay(0.5f);
    }
}
