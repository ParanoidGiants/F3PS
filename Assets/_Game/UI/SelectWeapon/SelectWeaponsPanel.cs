using System.Collections.Generic;
using UnityEngine;
using Weapon;

public class SelectWeaponsPanel : MonoBehaviour
{
    public GameObject selectableWeaponsParent;
    public GameObject selectableWeaponUIPrefab;
    public List<SelectableWeaponEntry> selectableWeaponUIs;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private int _activeWeaponIndex = 0;
    [SerializeField] private int _selectedWeaponIndex = -1;
    public Color activeWeaponColor;
    
    public void Init(WeaponManager weaponManager)
    {
        _weaponManager = weaponManager;
        for (int i = 0; i < _weaponManager.weapons.Count; i++)
        {
            var weapon = _weaponManager.weapons[i];
            var selectableWeaponUI = Instantiate(selectableWeaponUIPrefab, selectableWeaponsParent.transform).GetComponent<SelectableWeaponEntry>();
            selectableWeaponUI.Setup(weapon);
            selectableWeaponUIs.Add(selectableWeaponUI);
            if (weaponManager.IsActive(weapon))
            {
                _activeWeaponIndex = i;
                _selectedWeaponIndex = i;
            }
        }
        selectableWeaponsParent.SetActive(false);
    }

    public void SetActive(int activeWeaponIndex_)
    {
        _activeWeaponIndex = activeWeaponIndex_;
        Color active = activeWeaponColor;
        Color inactive = activeWeaponColor;
        inactive.a = 0f;
        for (int i = 0; i < selectableWeaponUIs.Count; i++)
        {
            var selectableWeaponUI = selectableWeaponUIs[i];
            selectableWeaponUI.Update();
            if (i == _activeWeaponIndex)
            {
                selectableWeaponUI.Select();
                selectableWeaponUI.Highlight(activeWeaponColor);
            }
            else
            {
                selectableWeaponUI.Unselect();
                selectableWeaponUI.Highlight(inactive);
            }
        }
        selectableWeaponsParent.SetActive(true);
    }

    public void SelectNextWeapon()
    {
        selectableWeaponUIs[_selectedWeaponIndex].Unselect();
        _selectedWeaponIndex = (_selectedWeaponIndex + 1) % selectableWeaponUIs.Count;
        selectableWeaponUIs[_selectedWeaponIndex].Select();
    }

    public int RetrieveSelection()
    {
        return _selectedWeaponIndex;
    }

    public void SetInactive()
    {
        selectableWeaponsParent.SetActive(false);
    }

    public void SelectPreviousWeapon()
    {
        selectableWeaponUIs[_selectedWeaponIndex].Unselect();
        _selectedWeaponIndex = (_selectedWeaponIndex + selectableWeaponUIs.Count - 1) % selectableWeaponUIs.Count;
        selectableWeaponUIs[_selectedWeaponIndex].Select();
    }
}
