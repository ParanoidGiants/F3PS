using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

namespace F3PSCharacterController
{
    public class BaseGun : MonoBehaviour
    {
        private Camera _cam;
        
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

        public int CurrentMagazineAmount => currentMagazineAmount;
        public int TotalAmount => totalAmount;
        public float ReloadPercentage => reloadMagazineTime / reloadMagazineTimer;

        private void Start()
        {
            _cam = Camera.main;
            projectilePool.Init(projectilePrefab);
            projectilePool.transform.parent = null;

            totalAmount = maxAmmo;
        }
        
        private void Update()
        {
            meshHolder.rotation = _cam.transform.rotation;
        }

        public void OnShoot()
        {
            
            StartCoroutine(Shoot());
        }

        private IEnumerator Shoot()
        {
            isShooting = true;
            shootCoolDownTime = shootCoolDownTimer;
            currentMagazineAmount--;
            projectilePool.ShootBullet(
                projectileSpawn.position,
                meshHolder.rotation,
                shotSpeed
            );
            while (shootCoolDownTime > 0f)
            {
                shootCoolDownTime -= Time.deltaTime;
                yield return null;
            }
            isShooting = false;
        }
        
        public void OnReload(Action<float> updateCallback)
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
            updateCallback(reloadMagazineTime / reloadMagazineTimer);

            currentMagazineAmount += reloadAmount;
            totalAmount -= reloadAmount;
            isReloadingMagazine = false;
        }
    }
}

