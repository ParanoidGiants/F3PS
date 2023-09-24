using System;
using System.Collections;
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
        
        [Space(10)]
        [Header("Watchers")]
        public int totalAmount = 100;
        public int currentMagazineAmount = 10;
        public float shootCoolDownTime = 0.0f;
        public float reloadMagazineTime = 0.0f;
        public bool isReloadingMagazine = false;
        public WeaponUI weaponUI;
        
        public void InitForPlayer(Transform user, WeaponUI weaponUI_ = null)
        {
            Init(user);
            weaponUI = weaponUI_;
        }
        
        public void Init(Transform user)
        {
            projectilePool.Init(projectilePrefab, user);
            totalAmount = maxAmmo;
        }
        
        public void UpdateRotation(Quaternion rotation)
        {
            meshHolder.rotation = rotation;
        }

        public void OnShoot()
        {
            if (isShooting) return;
            
        }

        protected virtual IEnumerator HandleShoot()
        {
            yield return null;
        }
        
        private void OnReload(Action<float> updateCallback)
        {
            StartCoroutine(HandleReload(updateCallback));
        }
        
        private IEnumerator HandleReload(Action<float> updateCallback)
        {
            if (isReloadingMagazine) yield break;
            
            var reloadAmount = maxMagazineAmmo - currentMagazineAmount;
            reloadAmount = Mathf.Min(reloadAmount, totalAmount);
            if (reloadAmount <= 0) yield break;

            // TODO: Play reload sound
            // TODO: Play reload animation
            
            isReloadingMagazine = true;
            reloadMagazineTime = reloadMagazineTimer;
            while (reloadMagazineTime > 0f)
            {
                reloadMagazineTime -= Time.deltaTime;
                updateCallback(reloadMagazineTime / reloadMagazineTimer);
                yield return null;
            }
            reloadMagazineTime = 0f;
            updateCallback(0f);

            currentMagazineAmount += reloadAmount;
            totalAmount -= reloadAmount;
            updateCallback(0f);
            isReloadingMagazine = false;
        }

        private bool _wasShootingLastFrame = false;
        public virtual void HandleShoot(bool isShootingPressed)
        {
            if (isShooting || isReloadingMagazine) return;
            
            if (currentMagazineAmount <= 0)
            {
                // TODO: Play empty clip sound
                weaponUI?.OnShootEmptyClip();
            }
            else
            {
                StartCoroutine(HandleShoot());
                weaponUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
            }
        }

        public void HandleReload()
        {
            if (isReloadingMagazine) return;
            
            OnReload(x =>
            {
                weaponUI?.UpdateReload(x);
                    
                if (x <= 0f)
                {
                    weaponUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
                }
            });
        }
        protected virtual IEnumerator Shoot()
        {
            yield return null;
        }
    }
}

