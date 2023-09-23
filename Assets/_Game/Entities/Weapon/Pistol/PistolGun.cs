using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class PistolGun : BaseGun
    {
        [SerializeField] private bool _wasShootingPressedLastFrame = false;

        override
        public void HandleShoot(bool isShootingPressed)
        {
            if (!_wasShootingPressedLastFrame && isShootingPressed)
            {
                if (currentMagazineAmount <= 0)
                {
                    // TODO: Play empty clip sound
                    weaponUI?.OnShootEmptyClip();
                }
                else
                {
                    StartCoroutine(Shoot());
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
        protected IEnumerator Shoot()
        {
            isShooting = true;
            shootCoolDownTime = shootCoolDownTimer;
            currentMagazineAmount--;
            projectilePool.ShootBullet(
                projectileSpawn.position,
                meshHolder.rotation,
                shotSpeed
            );
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

