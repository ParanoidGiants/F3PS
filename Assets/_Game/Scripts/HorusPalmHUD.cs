using System;
using UnityEngine;
using UnityEngine.UI;

public class HorusPalmHUD : MonoBehaviour
{
    public Image coolDownCircle;
    public Image icon;

    public void UpdateCoolDown(float percentage)
    {
        if (percentage == 0f)
        {
            coolDownCircle.fillAmount = 0f;
        }
        else
        {
            coolDownCircle.fillAmount = 1f - percentage;
        }
    }

    public void OnTryAttackWithoutStamina()
    {
        Debug.LogWarning("Horus Palm: Cannot attack without stamina!");
    }
}
