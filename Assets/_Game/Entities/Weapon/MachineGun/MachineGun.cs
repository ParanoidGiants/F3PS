using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class MachineGun : BaseGun
    {
        private Coroutine shootCoroutine;
        override
        public void HandleShoot(bool isShootingPressed)
        {
            if (!isShooting && isShootingPressed)
            {
                shootCoroutine = StartCoroutine(Shoot());
            }
            else if (isShooting && !isShootingPressed)
            {
                isShooting = false;
            }
        }


        override
        protected IEnumerator Shoot()
        {
            isShooting = true;
            shootCoolDownTime = shootCoolDownTimer;
            while (isShooting && !isReloadingMagazine)
            {
                shootCoolDownTime -= Time.deltaTime;
                if (shootCoolDownTime < 0f)
                {
                    shootCoolDownTime = shootCoolDownTimer;
                    
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
                            meshHolder.rotation,
                            shotSpeed
                        );
                        weaponUI?.UpdateAmmoText(currentMagazineAmount, totalAmount);
                        MasterAudio.PlaySound3DAtTransformAndForget("Weapon", transform);
                    }
                }
                yield return null;
            }
            isShooting = false;
        }
    }
}

