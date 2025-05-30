using System;
using UnityEngine;
using UnityEngine.UI;

public class MeleeAttackHud : MonoBehaviour
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

    public void ShowGrenade()
    {
        icon.gameObject.SetActive(true);
    }

    public void SetGrenadeVisible(bool visible)
    {
        icon.gameObject.SetActive(visible);
    }

    internal void OnTryAttackWithoutStamina()
    {
        throw new NotImplementedException();
    }
}
