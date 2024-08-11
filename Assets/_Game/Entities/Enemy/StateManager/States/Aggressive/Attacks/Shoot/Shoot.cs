using F3PS.Damage.Take;
using UnityEngine;
using Weapon;

namespace F3PS.AI.States.Action
{
    public class Shoot : Attack
    {
        [Space(10)]
        [Header("Shoot Settings")]
        public float rotationSpeed;
        
        [Space(10)]
        [Header("Shoot Watchers")]
        public BaseGun gun;
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
        public void OnStartAttack(Hittable hittable)
        {
            base.OnStartAttack(hittable);
            OnAttack();
        }

        override
        protected void OnAttack()
        {
            isAttacking = true;
        }

        override
        protected void HandleAttack()
        {
            UpdateGunAndEnemyRotation();
            _isShootingPressed = !_isShootingPressed;
            gun.HandleShoot(_isShootingPressed);
            
            
            if (gun.IsMagazineEmpty() || !IsTargetInLineOfSight(_target.Center()))
            {
                OnRecover();
            }
        }
        
        override
        protected void OnRecover()
        {
            isAttacking = false;
            isRecovering = true;
            gun.StartReloading();
            base.OnRecover();
        }

        
        override
        protected void HandleRecovering()
        {
            isRecovering = gun.isReloadingMagazine;
            if (isRecovering) return;
            
            OnStopAttacking();
        }
        
        private void UpdateGunAndEnemyRotation()
        {
            var targetPosition = _target.Center();
            var gunRotation = Quaternion.LookRotation(targetPosition - gun.transform.position);
            gun.UpdateRotation(gunRotation);
            UpdateEnemyRotation(targetPosition);
        }

        private void UpdateEnemyRotation(Vector3 targetPosition)
        {
            var enemyTransform = enemy.body.transform;
            var enemyUp = enemyTransform.up;
            var position = enemyTransform.position;
            var lookDirection = targetPosition - position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyUp);
            var targetRotation = Quaternion.LookRotation(newForward, enemyUp);
            
            enemyTransform.rotation = Quaternion.RotateTowards(
                enemyTransform.rotation,
                targetRotation,
                enemy.ScaledDeltaTime * rotationSpeed
            );
        }

        override
        public bool CanAttack(Hittable hittable)
        {
            return base.CanAttack(hittable) && IsTargetInLineOfSight(hittable.Center());
        }

        private bool IsTargetInLineOfSight(Vector3 targetPosition)
        {
            var position = gun.transform.position;
            var direction = (targetPosition - position).normalized;
            
            if (!Physics.Raycast(position, direction, out var hit, stoppingDistanceFollow, Helper.HittableLayer))
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
