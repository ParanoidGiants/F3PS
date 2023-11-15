using UnityEngine;
using Weapon;

namespace F3PS.AI.States.Action
{
    public class Shoot : Attack
    {
        [Space(10)]
        [Header("Shoot Settings")]
        public float chargeTimer;
        public float hitTimer;
        public float recoverTimer;
        public float rotationSpeed;
        
        [Space(10)]
        [Header("Shoot Watchers")]
        public BaseGun gun;
        public float chargeTime;
        public float hitTime;
        public float recoverTime;

        private bool _isShootingPressed = false;
        
        override
        public void Init(Material aggressiveMaterial)
        {
            gun = GetComponentInChildren<BaseGun>();
            gun.Init(enemy.body.transform.parent);        
            base.Init(aggressiveMaterial);
        }

        override
        protected void OnCharge()
        {
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;

            base.OnCharge();
        }

        override
        protected void HandleCharging()
        {
            UpdateGunAndEnemyRotation(_target.Center());
            chargeTime += enemy.ScaledDeltaTime;
            isCharging = chargeTime < chargeTimer;
            
            base.HandleCharging();
        }

        override
        protected void OnRecover()
        {
            gun.HandleReload();
            base.OnRecover();
        }
        
        override
        protected void HandleAttack()
        {
            UpdateGunAndEnemyRotation(_target.Center());
            hitTime += enemy.ScaledDeltaTime;
            isAttacking = hitTime < hitTimer;
            _isShootingPressed = !_isShootingPressed;
            gun.HandleShoot(_isShootingPressed);
            
            base.HandleAttack();
        }
        
        override
        protected void HandleRecovering()
        {
            UpdateEnemyRotation(_target.Center());
            recoverTime += enemy.ScaledDeltaTime;
            isRecovering = recoverTime < recoverTimer;
            
            base.HandleRecovering();
        }
        
        private void UpdateGunAndEnemyRotation(Vector3 targetPosition)
        {
            var gunRotation = Quaternion.LookRotation(targetPosition - gun.transform.position);
            gun.UpdateRotation(gunRotation);
            UpdateEnemyRotation(targetPosition);
        }

        private void UpdateEnemyRotation(Vector3 targetPosition)
        {
            var enemyTransform = enemy.body.transform;
            var position = enemyTransform.position;
            var lookDirection = targetPosition - position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
            var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, enemy.ScaledDeltaTime * rotationSpeed);
        }

        override
        public bool CanAttack(Vector3 targetPosition)
        {
            var position = gun.transform.position;
            var direction1 = (targetPosition - position).normalized;
            
            Debug.DrawRay(position, direction1 * attackDistance, Color.red);
            if (Physics.Raycast(position, direction1, out var hit, attackDistance, Helper.PlayerLayer))
            {
                if (Physics.Raycast(position, direction1, out hit, hit.distance, Helper.DefaultLayer))
                {
                    Debug.DrawRay(position, direction1 * hit.distance, Color.green);
                    return false;                
                } 
                return true;
            }
            return false;
        }
    }
}
