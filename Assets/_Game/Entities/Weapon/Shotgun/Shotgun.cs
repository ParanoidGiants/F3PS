using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class Shotgun : BaseGun
    {
        private bool _wasShootingPressedLastFrame = false;
        [Space(10)]
        [Header("Shotgun Settings")]
        [SerializeField] private int _numberOfProjectiles;
        [SerializeField] private float _spreadAngle;
        override
        public void HandleShoot(bool isShootingPressed, Vector3 targetPosition)
        {
            if (!_wasShootingPressedLastFrame && isShootingPressed)
            {
                if (currentMagazineAmount <= 0)
                {
                    // TODO: Play empty clip sound
                    weaponUI?.OnTryShootWithEmptyClip();
                }
                else
                {
                    StartCoroutine(Shoot(targetPosition));
                    weaponUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
                }
                _wasShootingPressedLastFrame = true;
            }
            else if (_wasShootingPressedLastFrame && !isShootingPressed && !isShooting)
            {
                _wasShootingPressedLastFrame = false;
            }
        }
        
        
        override
        protected IEnumerator Shoot(Vector3 targetPosition)
        {
            isShooting = true;
            shootCoolDownTime = shootCoolDownTimer;
            currentMagazineAmount--;
            
            for (int i = 0; i < _numberOfProjectiles; i++)
            {
                float xRotation = Random.Range(-_spreadAngle, _spreadAngle);
                float yRotation = Random.Range(-_spreadAngle, _spreadAngle);
                Quaternion projectileOrientation = Quaternion.Euler(xRotation, yRotation, 0f) * projectileSpawn.rotation;
                var targetDirection = projectileOrientation * Vector3.forward * Vector3.Magnitude(targetPosition - projectileSpawn.position);
                projectilePool.ShootBullet(
                    projectileSpawn.position,
                    projectileSpawn.position + targetDirection,
                    shotSpeed
                );
            }
            var shootDirection = targetPosition - projectileSpawn.position;
            Shake(-shootDirection);
            MasterAudio.PlaySound3DAtTransformAndForget("Weapon", transform);
            while (shootCoolDownTime > 0f && !isReloadingMagazine)
            {
                shootCoolDownTime -= Time.deltaTime;
                yield return null;
            }
            isShooting = false;
        }
    }
}

