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
        public int currentAmmo = 100;
        public int currentMagazineAmmo = 10;
        public float shootCoolDownTime = 0.0f;
        public float reloadMagazineTime = 0.0f;
        public bool isReloadingMagazine = false;

        public int CurrentMagazineAmmo => currentMagazineAmmo;
        public int CurrentAmmo => currentAmmo;
        public float ReloadPercentage => reloadMagazineTime / reloadMagazineTimer;

        private void Start()
        {
            _cam = Camera.main;
            projectilePool.Init(projectilePrefab);
            projectilePool.transform.parent = null;

            currentAmmo = maxAmmo;
        }
        
        private void Update()
        {
            meshHolder.rotation = _cam.transform.rotation;
        }

        public void OnShoot()
        {
            if (isShooting || isReloadingMagazine) return;
            
            if (currentMagazineAmmo <= 0)
            {
                // TODO: Play empty clip sound
                return;
            }
            
            StartCoroutine(Shoot());
        }

        private IEnumerator Shoot()
        {
            isShooting = true;
            shootCoolDownTime = shootCoolDownTimer;
            currentMagazineAmmo--;
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

        public void OnReload()
        {
            if (isReloadingMagazine) return;

            var reloadAmount = maxMagazineAmmo - currentMagazineAmmo;
            reloadAmount = Mathf.Min(reloadAmount, currentAmmo);
            
            if (reloadAmount <= 0) return;
            
            StartCoroutine(Reload(reloadAmount));
        }

        private IEnumerator Reload(int reloadAmount)
        {
            // TODO: Play reload sound
            // TODO: Play reload animation
            isReloadingMagazine = true;
            reloadMagazineTime = reloadMagazineTimer;
            while (reloadMagazineTime > 0f)
            {
                reloadMagazineTime -= Time.deltaTime;
                yield return null;
            }
            reloadMagazineTime = 0f;

            currentMagazineAmmo += reloadAmount;
            currentAmmo -= reloadAmount;
            isReloadingMagazine = false;
        }
    }
}

