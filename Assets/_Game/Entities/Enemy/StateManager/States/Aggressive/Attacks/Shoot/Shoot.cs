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
        public float requiredAngle;

        private bool _isShootingPressed = false;
        
        override
        public void Initialize(Material aggressiveMaterial)
        {
            gun = GetComponentInChildren<BaseGun>();
            gun.Init(enemy.body.transform.parent);        
            base.Initialize(aggressiveMaterial);
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
            var targetRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
            
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, targetRotation, enemy.ScaledDeltaTime * rotationSpeed);
        }

        override
        public bool CanAttack(Vector3 targetPosition)
        {
            var position = gun.transform.position;
            var direction = (targetPosition - position).normalized;
            
            if (!Physics.Raycast(position, direction, out var hit, attackDistance, Helper.PlayerLayer))
            {
                return false;
            }
            
            if (Physics.Raycast(position, direction, out hit, hit.distance, Helper.DefaultLayer))
            {
                return false;                
            }
    
            var bodyTransform = enemy.body.transform;
            var enemyPosition = bodyTransform.position;
            var enemyDirection = targetPosition - enemyPosition;
            var enemyForwardOnXZ = Vector3.ProjectOnPlane(bodyTransform.forward, Vector3.up);
            var enemyDirectionOnXZ = Vector3.ProjectOnPlane(enemyDirection, Vector3.up);
            var angle = Vector3.Angle(enemyForwardOnXZ, enemyDirectionOnXZ);
            return angle < requiredAngle;
        }
    }
}
