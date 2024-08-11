using System;
using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;

namespace Weapon
{
    public class BaseGun : MonoBehaviour
    {
        [Header("Bullet References")]
        public GameObject projectilePrefab;
        public Transform projectileSpawn;
        public ProjectilePool projectilePool;
        public Transform meshHolder;
        
        [Space(10)]
        [Header("Settings")]
        public Sprite icon;
        public int maxAmmo = 100;
        public int maxMagazineAmmo = 10;
        public float shotSpeed = 100f;
        public float shootCoolDownTimer = 0.2f;
        public float reloadMagazineTimer = 1f;
        public bool isShooting = false;

        [Space(10)] [Header("Watchers")]
        public bool isPlayer;
        public int totalAmount = 100;
        public int currentMagazineAmount = 10;
        public float shootCoolDownTime = 0.0f;
        public float reloadMagazineTime = 0.0f;
        public bool isReloadingMagazine = false;
        public WeaponUI weaponUI;
        
        protected virtual IEnumerator Shoot()
        {
            yield return null;
        }

        public void InitForPlayer(Transform user, WeaponUI weaponUI = null)
        {
            isPlayer = true;
            Init(user);
            this.weaponUI = weaponUI;
        }
        
        public void Init(Transform user)
        {
            projectilePool.Init(projectilePrefab, user, isPlayer);
            totalAmount = maxAmmo;
            currentMagazineAmount = maxMagazineAmmo;
        }
        
        public void UpdateRotation(Quaternion rotation)
        {
            meshHolder.rotation = rotation;
        }
        
        private IEnumerator HandleReload()
        {
            if (isReloadingMagazine) yield break;
            
            var reloadAmount = maxMagazineAmmo - currentMagazineAmount;
            reloadAmount = Mathf.Min(reloadAmount, totalAmount);
            if (reloadAmount <= 0) yield break;

            // MasterAudio.PlaySound3DAtTransformAndForget("SciFiWeapon_reload", transform);

            isReloadingMagazine = true;
            reloadMagazineTime = reloadMagazineTimer;
            while (reloadMagazineTime > 0f)
            {
                reloadMagazineTime -= Time.deltaTime;
                UpdateWeaponUI(reloadMagazineTime / reloadMagazineTimer);
                yield return null;
            }
            reloadMagazineTime = 0f;
            currentMagazineAmount += reloadAmount;
            totalAmount -= reloadAmount;
            UpdateWeaponUI();
            isReloadingMagazine = false;
        }

        public virtual void HandleShoot(bool isShootingPressed) {}

        public void StartReloading()
        {
            if (isReloadingMagazine) return;

            StartCoroutine(HandleReload());
        }

        protected void UpdateWeaponUI(float reloadPercentage = 0f)
        {
            if (!weaponUI) return;
            
            weaponUI.UpdateWeaponReload(reloadPercentage);
            if (reloadPercentage > 0f) return;
            
            weaponUI.UpdateAmmoText(currentMagazineAmount, totalAmount);
        }

        public bool IsMagazineEmpty()
        {
            return currentMagazineAmount <= 0;
        }
    }
}

