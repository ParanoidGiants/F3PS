using System;
using StarterAssets;
using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI ammoAmountText;
    private ThirdPersonController _playerController;
    private ThirdPersonController PlayerController
    {
        get
        {
            if (_playerController) return _playerController;

            _playerController = FindObjectOfType<ThirdPersonController>();
            return _playerController;
        }
    }

    private void Update()
    {
        UpdateAmmo(PlayerController.CurrentReloadAmmo, PlayerController.MaxAmmo);
    }

    public void UpdateAmmo(int currentReloadAmmo, int maxAmmo)
    {
        ammoAmountText.text = $"{currentReloadAmmo}/{maxAmmo}";
    }
}
