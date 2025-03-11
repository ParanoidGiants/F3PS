using System.Collections.Generic;
using System.Linq;
using F3PS;
using UnityEngine;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("References")]
        public WeaponUI weaponUI;
        public SelectWeaponsPanel selectWeaponsPanel;
        public ThrowTimeBubbleGrenade grenade;
        public List<BaseGun> weapons;
        private BaseGun _activeWeapon;

        [Header("Watchers")]
        public int _activeWeaponIndex = -1;
        public bool isInSwitchWeaponMode = false;
        public bool isSelecting = false;

        public void Init(Transform playerSpace)
        {
            foreach (var weapon in weapons)
            {
                weapon.Init(playerSpace);
                weapon.gameObject.SetActive(false);
            }
            selectWeaponsPanel.Init(this);
            grenade.weaponUI.SetGrenadeUIActive(false);
        }

        private void ChooseWeapon(int i)
        {
            weapons[_activeWeaponIndex].gameObject.SetActive(false);
            _activeWeaponIndex = i;
            _activeWeapon = weapons[i];
            _activeWeapon.gameObject.SetActive(true);
            
            weaponUI.UpdateAmmoText(
                _activeWeapon.currentMagazineAmount, 
                _activeWeapon.totalAmount
            );
            weaponUI.UpdateImage(_activeWeapon.icon);
        }

        public void OnUpdate(bool isAimingGrenade, bool isShooting, bool isReloading, Vector3 targetPosition)
        {
            if (_activeWeapon == null) return;

            if (grenade.HandleThrow(isAimingGrenade, targetPosition) || _activeWeapon.isReloadingMagazine)
            {
                return;
            }

            if (isReloading)
            {
                _activeWeapon.StartReloading();
            }
            else
            {
                _activeWeapon.HandleShoot(isShooting, targetPosition);
            }
        }

        public void OnFixedUpdate(Vector3 targetPosition)
        {
            if (_activeWeapon == null) return;

            var gunForward = targetPosition - _activeWeapon.transform.position;
            Quaternion gunRotation = Quaternion.identity * Quaternion.LookRotation(gunForward);
            _activeWeapon.UpdateRotation(gunRotation);
        }

        public void HandleSwitchWeapon(bool isSwitchingWeapon, float lookX)
        {
            if (weapons.Count(w => w.isUnlocked) <= 1 || _activeWeapon.isReloadingMagazine) return;

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

        public bool IsActive(BaseGun weapon)
        {
            return _activeWeapon != null && _activeWeapon == weapon;
        }
    }
}
