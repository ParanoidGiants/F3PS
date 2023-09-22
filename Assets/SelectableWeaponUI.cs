using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableWeaponUI : MonoBehaviour
{
    private Weapon.BaseGun _weapon;
    public Weapon.BaseGun Weapon => _weapon;
    public Image icon;
    public Image background;
    public TextMeshProUGUI magazineAmountText;
    public TextMeshProUGUI totalAmountText;
    
    public void Setup(Weapon.BaseGun weapon)
    {
        _weapon = weapon;
        icon.sprite = weapon.icon;
        Update();
    }

    public void Update()
    {
        magazineAmountText.text = _weapon.currentMagazineAmount.ToString();
        totalAmountText.text = _weapon.currentMagazineAmount.ToString();
    }

    public void Select()
    {
        Color c = background.color;
        c.a = 0.5f;
        background.color = c;
    }
    
    public void Unselect()
    {
        Color c = background.color;
        c.a = 0f;
        background.color = c;
    }
}
