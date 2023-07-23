using System.Collections;
using UnityEngine;

namespace F3PSCharacterController
{
    public class BaseGun : MonoBehaviour
    {
        [Header("Bullet References")]
        public GameObject projectilePrefab;
        public Transform projectileSpawn;
        public ProjectilePool projectilePool;

        public float shotSpeed = 100f;
        public float coolDownTimer = 0.2f;
        private float coolDownTime = 0.0f;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            projectilePool.Init(projectilePrefab);
            projectilePool.transform.parent = null;
        }

        public void OnShoot()
        {
            if (coolDownTime != 0f) return;
        
            coolDownTime = coolDownTimer;
            projectilePool.ShootBullet(
                projectileSpawn.position,
                _cam.transform.rotation,
                shotSpeed
            );;
            StartCoroutine(ApplyCoolDown());
        }

        private IEnumerator ApplyCoolDown()
        {
            while (coolDownTime > 0f)
            {
                coolDownTime -= Time.deltaTime;
                yield return null;
            }
            coolDownTime = 0f;
        }
        
        public void OnReload() {}
    }
}

