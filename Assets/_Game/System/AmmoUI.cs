using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI ammoAmountText;
    public Image ReloadCircle;
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
        UpdateAmmo(PlayerController.CurrentMagazineAmmo, PlayerController.CurrentAmmo);
        UpdateReload(PlayerController.ReloadPercentage);
    }

    public void UpdateAmmo(int currentReloadAmmo, int currentAmmo)
    {
        ammoAmountText.text = $"{currentReloadAmmo}/{currentAmmo}";
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
