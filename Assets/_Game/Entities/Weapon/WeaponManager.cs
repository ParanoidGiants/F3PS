using System.Collections.Generic;
using System.Linq;
using F3PS;
using Unity.VisualScripting;
using UnityEngine;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        public List<BaseGun> weapons;
        private BaseGun _activeWeapon;
        public BaseGun ActiveWeapon => _activeWeapon;
        [SerializeField] private int _activeWeaponIndex = -1;
        private bool isInSwitchWeaponMode = false;
        private bool isSelecting = false;
        private WeaponUI _weaponUI;
        private SelectWeaponsPanel _selectWeaponsPanel;
        private void Awake()
        {
            weapons = GetComponentsInChildren<BaseGun>().ToList();
            _weaponUI = FindObjectOfType<WeaponUI>();
            _selectWeaponsPanel = FindObjectOfType<SelectWeaponsPanel>();
        }

        public void Init(Transform playerSpace)
        {
            foreach (var weapon in weapons)
            {
                weapon.InitForPlayer(playerSpace, _weaponUI);
                weapon.gameObject.SetActive(false);
            }
            ChooseWeapon(0);
            _selectWeaponsPanel.Init(this);
        }

        private void ChooseWeapon(int i)
        {
            weapons[_activeWeaponIndex].gameObject.SetActive(false);
            _activeWeaponIndex = i;
            _activeWeapon = weapons[i];
            _activeWeapon.gameObject.SetActive(true);
            
            _weaponUI.UpdateAmmoText(
                ActiveWeapon.currentMagazineAmount, 
                ActiveWeapon.totalAmount
            );
            _weaponUI.UpdateImage(ActiveWeapon.icon);
        }

        public void OnUpdate(bool isShooting, bool isReloading, Quaternion transformRotation)
        {
            ActiveWeapon.HandleShoot(isShooting);
            if (isReloading) ActiveWeapon.HandleReload();
            
            ActiveWeapon.UpdateRotation(transformRotation);
        }

        public void HandleSwitchWeapon(bool isSwitchingWeapon, float lookX)
        {
            if (isSwitchingWeapon && !isInSwitchWeaponMode)
            {
                GameManager.Instance.PauseTime();
                // int nextWeaponIndex = (_activeWeaponIndex + 1) % weapons.Count;
                _selectWeaponsPanel.SetActive(_activeWeaponIndex);
                isInSwitchWeaponMode = true;
            }
            else if (!isSwitchingWeapon && isInSwitchWeaponMode)
            {
                GameManager.Instance.ResumeTime();
                ChooseWeapon(_selectWeaponsPanel.RetrieveSelection());
                _selectWeaponsPanel.SetInactive();
                isInSwitchWeaponMode = false;
                isSelecting = false;
            }
            else if (isInSwitchWeaponMode && !isSelecting)
            {
                Debug.Log(lookX);
                if (lookX > 0.1f)
                {
                    _selectWeaponsPanel.SelectNextWeapon();
                    isSelecting = true;
                }
                else if (lookX < -0.1f)
                {
                    _selectWeaponsPanel.SelectPreviousWeapon();
                    isSelecting = true;
                }
            }
            else if (isSelecting && lookX == 0f)
            {
                isSelecting = false;
            }
        }
    }
}
