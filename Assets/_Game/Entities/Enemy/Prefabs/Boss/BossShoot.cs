using F3PS.Damage.Take;
using F3PS.Enemy;
using UnityEngine;
using Weapon;

namespace F3PS.AI.States.Action
{
    public class BossShoot : Attack
    {
        [Space(10)]
        [Header("Shoot Settings")]
        public float rotationSpeed;
        
        [Space(10)]
        [Header("Shoot Watchers")]
        public BaseGun[] guns;
        public float requiredAngle;

        public float shootTime = 0f;
        public float shootTimer;
        private BossEnemy _boss;
        public Transform shooterLayer;
        
        override
        public void Initialize(Material aggressiveMaterial)
        {
            guns = shooterLayer.GetComponentsInChildren<BaseGun>();
            foreach (var gun in guns)
            {
                gun.Init(enemy.body.transform.parent);
            }
            base.Initialize(aggressiveMaterial);
            _boss = (BossEnemy) enemy;
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
            shootTime = shootTimer;
            isAttacking = true;
        }

        override
        protected void HandleAttack()
        {
            UpdateGunAndEnemyRotation();
            
            shootTime -= enemy.ScaledDeltaTime;
            if (shootTime > 0f) return;

            shootTime = shootTimer;
            bool isMagazineEmpty = false;
            foreach (var gun in guns)
            {
                gun.HandleShoot(true);
                gun.HandleShoot(false);
                isMagazineEmpty = isMagazineEmpty || gun.IsMagazineEmpty();
            }
            if (isMagazineEmpty || !IsTargetInLineOfSight(_target.Center()))
            {
                OnRecover();
            }
        }
        
        override
        protected void OnRecover()
        {
            isAttacking = false;
            isRecovering = true;

            foreach (var gun in guns)
            {
                gun.StartReloading();
            }
            base.OnRecover();
        }

        
        override
        protected void HandleRecovering()
        {
            isRecovering = guns[0].isReloadingMagazine;
            if (isRecovering) return;
            
            OnStopAttacking();
        }
        
        private void UpdateGunAndEnemyRotation()
        {
            var targetPosition = _target.Center();
            var gunRotation = Quaternion.LookRotation(targetPosition - _boss.center.position);
            foreach (var gun in guns)
            {
                gun.UpdateRotation(gunRotation);
            }
            UpdateEnemyRotation(targetPosition);
        }

        private void UpdateEnemyRotation(Vector3 targetPosition)
        {
            var enemyTransform = _boss.body.transform;
            var enemyUp = enemyTransform.up;
            var position = enemyTransform.position;
            var lookDirection = targetPosition - position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyUp);
            var targetRotation = Quaternion.LookRotation(newForward, enemyUp);
            
            enemyTransform.rotation = Quaternion.RotateTowards(
                enemyTransform.rotation,
                targetRotation,
                _boss.ScaledDeltaTime * rotationSpeed
            );
        }

        override
        public bool CanAttack(Hittable hittable)
        {
            return base.CanAttack(hittable) && IsTargetInLineOfSight(hittable.Center());
        }

        private bool IsTargetInLineOfSight(Vector3 targetPosition)
        {
            var position = _boss.center.position;
            var direction = (targetPosition - position).normalized;
            
            if (!Physics.Raycast(position, direction, out var hit, stoppingDistanceFollow, Helper.HittableLayer))
            {
                return false;
            }
            
            if (Physics.Raycast(position, direction, out hit, hit.distance, Helper.DefaultLayer))
            {
                return false;
            }

            var bodyTransform = _boss.body.transform;
            var enemyPosition = bodyTransform.position;
            var enemyDirection = targetPosition - enemyPosition;
            var enemyForwardOnXZ = Vector3.ProjectOnPlane(bodyTransform.forward, Vector3.up);
            var enemyDirectionOnXZ = Vector3.ProjectOnPlane(enemyDirection, Vector3.up);
            var angle = Vector3.Angle(enemyForwardOnXZ, enemyDirectionOnXZ);
            return angle < requiredAngle;
        }
    }
}
