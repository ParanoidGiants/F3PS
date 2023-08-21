using UnityEngine;
using Player;

namespace Enemy.States
{
    public class Shoot : Attack
    {
        [Space(10)]
        [Header("Shoot References")]
        public BaseGun gun;
        
        [Space(10)]
        [Header("Shoot Settings")]
        public float chargeTimer;
        public float hitTimer;
        public float recoverTimer;
        
        [Space(10)]
        [Header("Shoot Watchers")]
        public float chargeTime;
        public float hitTime;
        public float recoverTime;
        
        private void Start()
        {
            enemy = navMeshAgent.GetComponent<BaseEnemy>();
        }
        
        private void EarlyHit()
        {
            OnRecover();
        }

        override
        protected void OnCharge()
        {
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;

            base.OnCharge();
        }
        
        private void UpdateGunRotation()
        {
            var targetPosition = _target.Center();
            var gunRotation = Quaternion.LookRotation(targetPosition - gun.transform.position);
            gun.UpdateRotation(gunRotation);
            
            var enemyTransform = enemy.transform;
            var position = enemyTransform.position;
            var lookDirection = targetPosition - position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
            var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
        }

        override
        protected void HandleCharging()
        {
            UpdateGunRotation();
            chargeTime += Time.deltaTime;
            isCharging = chargeTime < chargeTimer;
            base.HandleCharging();
        }

        override
        protected void HandleHitting()
        {
            UpdateGunRotation();
            hitTime += Time.deltaTime;
            isHitting = hitTime < hitTimer;
            gun.Shoot();
            base.HandleHitting();
        }

        private Vector3 _recoverStartPosition;
        private Vector3 _recoverEndPosition;
        private Vector3 _recoverForward;
        override
        protected void OnRecover()
        {
            gun.Reload();
            base.OnRecover();
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            isRecovering = recoverTime < recoverTimer;
            base.HandleRecovering();
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
