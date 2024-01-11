using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _reloadPercentage;
    private Animator _animator;

    [Header("Time Grenade")]
    public Image grenadeEffectCircle;
    public Image grenadeIcon;

    [Space(10)]
    [Header("Primary Weapon")]
    public TextMeshProUGUI magazineAmountText;
    public TextMeshProUGUI magazineAmountTextDuplicate;
    public TextMeshProUGUI totalAmountText;
    public Image weaponReloadCircle;
    public Image weaponIcon;
    
    private static readonly int Pulsate = Animator.StringToHash("pulsate");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void UpdateWeaponReload(float percentage)
    {
        if (percentage == 0f)
        {
            weaponReloadCircle.fillAmount = 0f;
        }
        else
        {
            weaponReloadCircle.fillAmount = 1f - percentage;
        }
    }
    
    public void UpdateGrenadeEffect(float percentage)
    {
        if (percentage == 0f)
        {
            grenadeEffectCircle.fillAmount = 0f;
        }
        else
        {
            grenadeEffectCircle.fillAmount = 1f - percentage;
        }
    }

    public void OnTryShootWithEmptyClip()
    {
        _animator.SetTrigger(Pulsate);
    }

    public void UpdateAmmoText(int baseGunCurrentMagazineAmmo, int baseGunCurrentAmmo)
    {
        magazineAmountText.text = baseGunCurrentMagazineAmmo.ToString();
        magazineAmountTextDuplicate.text = baseGunCurrentMagazineAmmo.ToString();
        totalAmountText.text = baseGunCurrentAmmo.ToString();
        StartCoroutine(Helper.UpdateLayoutGroups(_rectTransform));
    }

    public void UpdateImage(Sprite activeWeaponIcon)
    {
        weaponIcon.sprite = activeWeaponIcon;
    }

    public void SetGrenadeUIActive(bool isGrenadeActive)
    {
        grenadeIcon.gameObject.SetActive(isGrenadeActive);
    }
}
