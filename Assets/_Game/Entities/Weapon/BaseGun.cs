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

        public float shotSpeed = 100f;
        public float shootCoolDownTimer = 0.2f;
        private float shootCoolDownTime = 0.0f;
        public bool isShooting = false; 
        
        private int maxAmmo = 100;
        private int currentAmmo = 100;
        private int maxReloadedAmmo = 10;
        private int _currentMagazineAmmo = 10;
        public float reloadTimer = 1f;
        private float reloadTime = 0.0f;
        public bool isReloading = false;

        public int CurrentAmmo => _currentMagazineAmmo;
        public int MaxAmmo => maxAmmo;

        private void Start()
        {
            _cam = Camera.main;
            projectilePool.Init(projectilePrefab);
            projectilePool.transform.parent = null;
        }

        public void OnShoot()
        {
            if (isShooting) return;
            if (_currentMagazineAmmo <= 0)
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
            _currentMagazineAmmo--;
            projectilePool.ShootBullet(
                projectileSpawn.position,
                _cam.transform.rotation,
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
            if (isReloading) return;
            
            StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            // TODO: Play reload sound
            // TODO: Play reload animation
            isReloading = true;
            reloadTime = reloadTimer;
            while (reloadTime > 0f)
            {
                reloadTime -= Time.deltaTime;
                yield return null;
            }
            _currentMagazineAmmo = Mathf.Min(currentAmmo, maxReloadedAmmo);
            currentAmmo -= _currentMagazineAmmo;
            _currentMagazineAmmo = Mathf.Min(currentAmmo, maxReloadedAmmo);
            currentAmmo -= _currentMagazineAmmo;
            isReloading = false;
        }
        
        public void AddAmmo(int amount)
        {
            currentAmmo += amount;
            currentAmmo = Mathf.Min(currentAmmo, maxAmmo);
        }
    }
}

