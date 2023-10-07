using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _reloadPercentage;
    private Animator _animator;

    public TextMeshProUGUI magazineAmountText;
    public TextMeshProUGUI magazineAmountTextDuplicate;
    public TextMeshProUGUI totalAmountText;
    public Image reloadCircle;
    public Image weaponIcon;
    private static readonly int Pulsate = Animator.StringToHash("pulsate");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void UpdateReload(float percentage)
    {
        if (percentage == 0f)
        {
            reloadCircle.fillAmount = 0f;
        }
        else
        {
            reloadCircle.fillAmount = 1f - percentage;
        }
    }

    public void OnShootEmptyClip()
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
}
