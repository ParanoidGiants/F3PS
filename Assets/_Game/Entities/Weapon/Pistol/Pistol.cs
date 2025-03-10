using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class Pistol : BaseGun
    {
        [SerializeField] private bool _wasShootingPressedLastFrame = false;
        private Vector3 currentTargetPosition;

        override
        public void HandleShoot(bool isShootingPressed, Vector3 targetPosition)
        {
            currentTargetPosition = targetPosition;
            if (!_wasShootingPressedLastFrame && isShootingPressed)
            {
                if (IsMagazineEmpty())
                {
                    weaponUI?.OnTryShootWithEmptyClip();
                }
                else
                {
                    StartCoroutine(Shoot(targetPosition));
                    UpdateWeaponUI();
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
            projectilePool.ShootBullet(
                projectileSpawn.position,
                targetPosition,
                shotSpeed
            );
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

