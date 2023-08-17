using UnityEngine;
using F3PSCharacterController;

namespace Enemy.States
{
    public class Shoot : Attack
    {
        [Space(10)]
        [Header("Shoot Settings")]
        public BaseGun gun;
        public float chargeTime;
        public float chargeTimer;
        
        public float hitTime;
        public float hitTimer;
        
        public float recoverTime;
        public float recoverTimer;
        
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
    }
}
