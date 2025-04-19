using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeBubbleHUD : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Animator _animator;

    public Image lifeTimeCircle;
    public Image icon;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
    }

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
