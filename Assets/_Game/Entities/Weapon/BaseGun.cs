using DarkTonic.MasterAudio;
using System;
using System.Collections;
using UnityEngine;

namespace Player
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

        protected virtual IEnumerator Shoot()
        {
            yield return null;
        }
        
        private void OnReload(Action<float> updateCallback)
        {
            StartCoroutine(Reload(updateCallback));
        }
        
        private IEnumerator Reload(Action<float> updateCallback)
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

        public void Shoot(AmmoUI _ammoUI = null)
        {
            if (isShooting || isReloadingMagazine) return;
            
            if (currentMagazineAmount <= 0)
            {
                // TODO: Play empty clip sound
                _ammoUI?.OnShootEmptyClip();
            }
            else
            {
                StartCoroutine(Shoot());
                _ammoUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
            }
        }

        public void Reload(AmmoUI _ammoUI = null)
        {
            if (isReloadingMagazine) return;
            
            OnReload(x =>
            {
                _ammoUI?.UpdateReload(x);
                    
                if (x <= 0f)
                {
                    _ammoUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
                }
            });
        }
    }
}

