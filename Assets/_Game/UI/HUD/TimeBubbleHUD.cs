using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeBubbleHUD : MonoBehaviour
{
    public Image lifeTimeCircle;
    public Image icon;

    public void UpdateGrenadeEffect(float percentage)
    {
        if (percentage == 0f)
        {
            lifeTimeCircle.fillAmount = 0f;
        }
        else
        {
            lifeTimeCircle.fillAmount = 1f - percentage;
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
}
