using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        public List<BaseGun> weapons;
        private BaseGun _activeWeapon;
        public BaseGun ActiveWeapon => _activeWeapon;
        
        private void Awake()
        {
            weapons = GetComponentsInChildren<BaseGun>().ToList();
        }

        public void Init(Transform playerSpace)
        {
            foreach (var weapon in weapons)
            {
                weapon.Init(playerSpace);
                weapon.gameObject.SetActive(false);
            }
            ChooseWeapon(0);
        }

        private void ChooseWeapon(int i)
        {
            _activeWeapon = weapons[i];
            _activeWeapon.gameObject.SetActive(true);
        }

        public void ShootAndReload(bool isShooting, bool isReloading, AmmoUI ammoUI, Quaternion transformRotation)
        {
            
            if (isShooting) ActiveWeapon.Shoot(ammoUI);
            if (isReloading) ActiveWeapon.Reload(ammoUI);
            ActiveWeapon.UpdateRotation(transformRotation);
        }
    }
}
