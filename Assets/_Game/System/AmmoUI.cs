using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private ThirdPersonController _playerController;
    private int _magazineAmount;
    private int _totalAmount;
    private float _reloadPercentage;
    
    public TextMeshProUGUI magazineAmountText;
    public TextMeshProUGUI magazineAmountTextDuplicate;
    public TextMeshProUGUI totalAmountText;
    public Image ReloadCircle;

    private ThirdPersonController PlayerController
    {
        get
        {
            if (_playerController) return _playerController;

            _playerController = FindObjectOfType<ThirdPersonController>();
            return _playerController;
        }
    }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (_magazineAmount != PlayerController.CurrentMagazineAmmo || _totalAmount != PlayerController.CurrentAmmo)
        {
            UpdateAmmo(PlayerController.CurrentMagazineAmmo, PlayerController.CurrentAmmo);
        }
        if (Math.Abs(_reloadPercentage - PlayerController.ReloadPercentage) > Mathf.Epsilon)
        {
            UpdateReload(PlayerController.ReloadPercentage);
        }
    }

    public void UpdateAmmo(int currentMagazineAmount, int currentTotalAmount)
    {
        magazineAmountText.text = currentMagazineAmount.ToString();
        magazineAmountTextDuplicate.text = currentMagazineAmount.ToString();
        totalAmountText.text = currentTotalAmount.ToString();
        _magazineAmount = currentMagazineAmount;
        _totalAmount = currentTotalAmount;
        StartCoroutine(Helper.UpdateLayoutGroups(_rectTransform));
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
}
