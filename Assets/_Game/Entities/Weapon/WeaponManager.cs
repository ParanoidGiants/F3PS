using System;
using System.Collections.Generic;
using System.Linq;
using F3PS;
using UnityEngine;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("References")]
        public Transform playerSpace;
        public WeaponUI weaponUI;
        public SelectWeaponsPanel selectWeaponsPanel;
        public Crosshair crosshair;
        public ThrowTimeBubbleGrenade grenade;
        public List<BaseGun> weapons;
        public BaseGun _selectedWeapon;
        

        [Header("Watchers")]
        public int _activeWeaponIndex = -1;
        public bool isInSwitchWeaponMode = false;
        public bool isSelecting = false;
        public bool isOneWeaponUnlocked;
        
        private Vector3 _aimTargetPosition;

        public void Init()
        {
            foreach (var weapon in weapons)
            {
                weapon.Init(playerSpace);
            }
            _selectedWeapon = weapons.FirstOrDefault(w => w.isUnlocked);
            selectWeaponsPanel.Init();
            weaponUI.SetGrenadeVisible(grenade.isUnlocked);
            crosshair.gameObject.SetActive(isOneWeaponUnlocked);
        }

        private void ChooseWeapon(int i)
        {
            weapons[_activeWeaponIndex].gameObject.SetActive(false);
            _activeWeaponIndex = i;
            _selectedWeapon = weapons[i];
            _selectedWeapon.gameObject.SetActive(true);
            
            weaponUI.UpdateAmmoText(
                _selectedWeapon.currentMagazineAmount, 
                _selectedWeapon.totalAmount
            );
            weaponUI.UpdateImage(_selectedWeapon.icon);
        }

        public void OnUpdate(bool isAimingGrenade, bool isShooting, bool isReloading)
        {
            isOneWeaponUnlocked = weapons.Any(w => w.isUnlocked);
            if (!isOneWeaponUnlocked) return;

            // TODO: Refactor for crosshair to only ray cast once per frame and only when needed
            if (grenade.HandleThrow(isAimingGrenade, _aimTargetPosition) || _selectedWeapon.isReloadingMagazine)
            {
                return;
            }

            if (isReloading)
            {
                _selectedWeapon.StartReloading();
            }
            else
            {
                _selectedWeapon.HandleShoot(isShooting, _aimTargetPosition);
            }
        }

        public void OnFixedUpdate()
        {
            if (!isOneWeaponUnlocked) return;
            _aimTargetPosition = crosshair.GetTargetPosition();
            var gunForward = _aimTargetPosition - _selectedWeapon.transform.position;
            Quaternion gunRotation = Quaternion.identity * Quaternion.LookRotation(gunForward);
            _selectedWeapon.UpdateRotation(gunRotation);
        }

        public void HandleSwitchWeapon(bool isSwitchingWeapon, float lookX)
        {
            if (!isOneWeaponUnlocked || _selectedWeapon.isReloadingMagazine) return;

            if (isSwitchingWeapon && !isInSwitchWeaponMode)
            {
                GameManager.Instance.timeManager.PauseTime();
                selectWeaponsPanel.SetActive(_activeWeaponIndex);
                isInSwitchWeaponMode = true;
            }
            else if (!isSwitchingWeapon && isInSwitchWeaponMode)
            {
                GameManager.Instance.timeManager.ResumeTime();
                ChooseWeapon(selectWeaponsPanel.RetrieveSelection());
                selectWeaponsPanel.SetInactive();
                isInSwitchWeaponMode = false;
                isSelecting = false;
            }
            else if (isInSwitchWeaponMode && !isSelecting)
            {
                if (lookX > 0.1f)
                {
                    selectWeaponsPanel.SelectNextWeapon();
                    isSelecting = true;
                }
                else if (lookX < -0.1f)
                {
                    selectWeaponsPanel.SelectPreviousWeapon();
                    isSelecting = true;
                }
            }
            else if (isSelecting && lookX == 0f)
            {
                isSelecting = false;
            }
        }

        public bool IsSelected(BaseGun weapon)
        {
            return _selectedWeapon != null && _selectedWeapon == weapon;
        }

        public void UnlockPistol()
        {
            weapons[0].Unlock();
            ChooseWeapon(0);
            weaponUI.SetGunVisible(true);
            crosshair.gameObject.SetActive(true);
        }
    }
}
