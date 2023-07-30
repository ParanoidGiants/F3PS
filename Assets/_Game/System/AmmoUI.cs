using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private int _magazineAmount;
    private int _totalAmount;
    private float _reloadPercentage;
    private Animator _animator;
    
    public TextMeshProUGUI magazineAmountText;
    public TextMeshProUGUI magazineAmountTextDuplicate;
    public TextMeshProUGUI totalAmountText;
    public Image ReloadCircle;
    private static readonly int Pulsate = Animator.StringToHash("pulsate");

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void UpdateReload(float percentage)
    {
        if (percentage == 0f)
        {
            ReloadCircle.fillAmount = 0f;
        }
        else
        {
            ReloadCircle.fillAmount = 1f - percentage;
        }
    }

    public void OnShootEmptyClip()
    {
        _animator.SetTrigger(Pulsate);
    }

    public void OnShoot(int baseGunCurrentMagazineAmmo, int baseGunCurrentAmmo)
    {
        magazineAmountText.text = baseGunCurrentMagazineAmmo.ToString();
        magazineAmountTextDuplicate.text = baseGunCurrentMagazineAmmo.ToString();
        totalAmountText.text = baseGunCurrentAmmo.ToString();
        _magazineAmount = baseGunCurrentMagazineAmmo;
        _totalAmount = baseGunCurrentAmmo;
        StartCoroutine(Helper.UpdateLayoutGroups(_rectTransform));
    }
}
