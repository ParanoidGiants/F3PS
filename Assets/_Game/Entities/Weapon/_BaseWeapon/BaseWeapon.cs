using System;
using System.Collections;
using Cinemachine;
using DarkTonic.MasterAudio;
using UnityEngine;

namespace Weapon
{
    public class BaseGun : MonoBehaviour
    {
        [Header("References")]
        public Transform meshHolder;
        public CinemachineImpulseSource screenShakeSource;
        
        [Space(10)]
        [Header("Projectile References")]
        public GameObject projectilePrefab;
        public Transform projectileSpawn;
        public ProjectilePool projectilePool;

        [Space(10)]
        [Header("Settings")]
        public Sprite icon;
        public int maxAmmo = 100;
        public int maxMagazineAmmo = 10;
        public float shotSpeed = 100f;
        public float shootCoolDownTimer = 0.2f;
        public float reloadMagazineTimer = 1f;
        public bool isShooting = false;
        public float recoilPower;

        [Space(10)] [Header("Watchers")]
        public int totalAmount = 100;
        public int currentMagazineAmount = 10;
        public float shootCoolDownTime = 0.0f;
        public float reloadMagazineTime = 0.0f;
        public bool isReloadingMagazine = false;
        public WeaponUI weaponUI;

        protected virtual IEnumerator Shoot(Vector3 targetPosition)
        {
            yield return null;
        }

        protected void Shake(Vector3 recoilDirection)
        {
            screenShakeSource.GenerateImpulseWithVelocity(recoilDirection * recoilPower);
        }

        public void SetWeaponUI(WeaponUI weaponUI) { this.weaponUI = weaponUI; }
        public void Init(Transform userSpace)
        {
            projectilePool.Init(projectilePrefab, userSpace);
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

        public virtual void HandleShoot(bool isShootingPressed, Vector3 targetPosition) {}

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

