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
        private bool _wasSwitchingWeaponsLastFrame = false;
        private bool _wasSelectingAWeaponLastFrame = false;
        private WeaponUI _weaponUI;
        private SwitchWeaponsPanel _switchWeaponsPanel;
        private void Awake()
        {
            weapons = GetComponentsInChildren<BaseGun>().ToList();
            _weaponUI = FindObjectOfType<WeaponUI>();
            _switchWeaponsPanel = FindObjectOfType<SwitchWeaponsPanel>();
        }

        public void Init(Transform playerSpace)
        {
            foreach (var weapon in weapons)
            {
                weapon.Init(playerSpace);
                weapon.gameObject.SetActive(false);
            }
            ChooseWeapon(0);
            _switchWeaponsPanel.Init(this);
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

        public void ShootAndReload(bool isShooting, bool isReloading, Quaternion transformRotation)
        {
            if (isShooting) ActiveWeapon.Shoot(_weaponUI);
            if (isReloading) ActiveWeapon.Reload(_weaponUI);
            
            ActiveWeapon.UpdateRotation(transformRotation);
        }

        public void HandleSwitchWeapon(bool isSwitchingWeapon, float lookX)
        {
            if (isSwitchingWeapon && !_wasSwitchingWeaponsLastFrame)
            {
                GameManager.Instance.PauseTime();
                // int nextWeaponIndex = (_activeWeaponIndex + 1) % weapons.Count;
                _switchWeaponsPanel.SetActive(_activeWeaponIndex);
                _wasSwitchingWeaponsLastFrame = true;
            }
            else if (!isSwitchingWeapon && _wasSwitchingWeaponsLastFrame)
            {
                GameManager.Instance.ResumeTime();
                ChooseWeapon(_switchWeaponsPanel.RetrieveSelection());
                _switchWeaponsPanel.SetInactive();
                _wasSwitchingWeaponsLastFrame = false;
            }
            else if (_wasSwitchingWeaponsLastFrame && !_wasSelectingAWeaponLastFrame)
            {
                if (lookX > 10f)
                {
                    _switchWeaponsPanel.SelectNextWeapon();
                    _wasSelectingAWeaponLastFrame = true;
                }
                else if (lookX < -10f)
                {
                    _switchWeaponsPanel.SelectPreviousWeapon();
                    _wasSelectingAWeaponLastFrame = true;
                }
            }
            else if (_wasSelectingAWeaponLastFrame && lookX == 0f)
            {
                _wasSelectingAWeaponLastFrame = false;
            }
        }
        
        public void SwitchWeapon()
        {
            _switchWeaponsPanel.SelectNextWeapon();
        }
    }
}
