using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class MachineGun : BaseGun
    {
        private Vector3 _recentTargetPosition;
        override
        public void HandleShoot(bool isShootingPressed, Vector3 targetPosition)
        {
            _recentTargetPosition = targetPosition;
            if (!isShooting && isShootingPressed)
            {
                StartCoroutine(Shoot(targetPosition));
            }
            else if (isShooting && !isShootingPressed)
            {
                isShooting = false;
            }
        }


        override
        protected IEnumerator Shoot(Vector3 targetPosition)
        {
            isShooting = true;
            var waitForShootCoolDown = new WaitForSeconds(shootCoolDownTimer);
            while (isShooting && !isReloadingMagazine)
            {
                if (currentMagazineAmount <= 0)
                {
                    // TODO: Play empty clip sound
                    weaponUI?.OnTryShootWithEmptyClip();
                }
                else
                {
                    currentMagazineAmount--;
                    projectilePool.ShootBullet(
                        projectileSpawn.position,
                        _recentTargetPosition,
                        shotSpeed
                    );
                    var shootDirection = _recentTargetPosition - projectileSpawn.position;
                    weaponUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
                    Shake(-shootDirection);
                    MasterAudio.PlaySound3DAtTransformAndForget("Weapon", transform);
                }
                yield return waitForShootCoolDown;
            }
            isShooting = false;
        }
    }
}

