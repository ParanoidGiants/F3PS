using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class Pistol : BaseProjectileShooter
    {
        [SerializeField] private bool _wasShootingPressedLastFrame = false;

        override
        public void HandleShoot(bool isShootingPressed, Vector3 targetPosition)
        {
            if (!_wasShootingPressedLastFrame && isShootingPressed)
            {
                if (IsMagazineEmpty())
                {
                    skillUI?.OnTryShootWithEmptyClip();
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

