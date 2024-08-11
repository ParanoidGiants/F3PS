using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class Pistol : BaseGun
    {
        [SerializeField] private bool _wasShootingPressedLastFrame = false;

        override
        public void HandleShoot(bool isShootingPressed)
        {
            if (!_wasShootingPressedLastFrame && isShootingPressed)
            {
                if (IsMagazineEmpty())
                {
                    weaponUI?.OnTryShootWithEmptyClip();
                }
                else
                {
                    StartCoroutine(Shoot());
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

