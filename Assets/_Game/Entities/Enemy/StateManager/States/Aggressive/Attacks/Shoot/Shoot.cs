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
        
        [Space(10)]
        [Header("Shoot Watchers")]
        public BaseGun gun;
        public float chargeTime;
        public float hitTime;
        public float recoverTime;

        override
        public void Init()
        {
            gun = GetComponentInChildren<BaseGun>();
            gun.Init(enemy.body.transform.parent);            
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
        protected void OnRecover()
        {
            gun.HandleReload();
            base.OnRecover();
        }

        override
        protected void HandleCharging()
        {
            UpdateGunAndEnemyRotation();
            chargeTime += Time.deltaTime;
            isCharging = chargeTime < chargeTimer;
            
            base.HandleCharging();
        }

        private bool isShootingPressed = false;
        override
        protected void HandleHitting()
        {
            UpdateGunAndEnemyRotation();
            hitTime += Time.deltaTime;
            isHitting = hitTime < hitTimer;
            isShootingPressed = !isShootingPressed;
            gun.HandleShoot(isShootingPressed);
            
            base.HandleHitting();
        }
        
        override
        protected void HandleRecovering()
        {
            UpdateGunAndEnemyRotation();
            recoverTime += Time.deltaTime;
            isRecovering = recoverTime < recoverTimer;
            
            base.HandleRecovering();
        }
        
        private void UpdateGunAndEnemyRotation()
        {
            var targetPosition = _target.Center();
            var gunRotation = Quaternion.LookRotation(targetPosition - gun.transform.position);
            gun.UpdateRotation(gunRotation);
            
            var enemyTransform = enemy.body.transform;
            var position = enemyTransform.position;
            var lookDirection = targetPosition - position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
            var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
        }
        
        override
        public bool IsInAttackDistance(Vector3 targetPosition)
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
