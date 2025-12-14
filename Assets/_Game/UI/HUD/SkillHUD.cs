using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillHUD : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Animator _animator;

    [Header("Khonsu Sphere Projectile")]
    public Image projectileEffectCircle;
    public Image projectileIcon;

    [Space(10)]
    [Header("Gun")]
    public GameObject gunParent;
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
    
    public void UpdateProjectileEffect(float percentage)
    {
        if (percentage == 0f)
        {
            projectileEffectCircle.fillAmount = 0f;
        }
        else
        {
            projectileEffectCircle.fillAmount = 1f - percentage;
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

    public void SetGunVisible(bool visible)
    {
        gunParent.SetActive(visible);
    }
}
