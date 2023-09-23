using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableWeaponEntry : MonoBehaviour
{
    private Weapon.BaseGun _weapon;
    public Weapon.BaseGun Weapon => _weapon;
    public Image icon;
    public Image activeBackground;
    public Image selectBackground;
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
        totalAmountText.text = _weapon.totalAmount.ToString();
    }
    
    public void Highlight(Color backgroundColor)
    {
        activeBackground.color = backgroundColor;
    }

    public void Select()
    {
        Color c = selectBackground.color;
        c.a = 0.25f;
        selectBackground.color = c;
    }
    
    public void Unselect()
    {
        Color c = selectBackground.color;
        c.a = 0f;
        selectBackground.color = c;
    }
}
