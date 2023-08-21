using UnityEngine;

namespace Enemy.States
{
    public class Aggressive : State
    {
        private bool _isAttacking;
        private Hittable _target;
        private Attack _nextAttack;
        
        [Space(10)]
        [Header("Specific Settings")]
        public Vision aggressiveVision;
        public AggressiveSensor aggressiveSensor;
        public Attack[] attacks;
        
        private void Update()
        {
            foreach (var attack in attacks)
            {
                if (attack.HasCooledDown()) continue;
                attack.CoolDown();
            }
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            aggressiveVision.gameObject.SetActive(true);
            aggressiveSensor.gameObject.SetActive(true);
            _target = GetTargetFromSensors();
            // set default vision to false after getting the target from the sensors
            defaultVision.gameObject.SetActive(false);
            
            _nextAttack = NextAttack();
            HandlePositionAndRotation(false, false);
        }


        public bool IsTargetDetected()
        {
            return defaultVision.canTargetBeDetected 
                   || aggressiveVision.canTargetBeDetected
                   || aggressiveVision.triggerCount > 0;
        }

        override
        public void OnUpdate()
        {
            
            if (!_isAttacking && !IsTargetDetected())
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }

            bool hasReachedDestination = Helper.HasReachedDestination(navMeshAgent);
            bool attackHasCooledDown = _nextAttack.HasCooledDown();
            bool isInAttackDistance = _nextAttack.IsInAttackDistance(_target.Center());
            bool canAttack = attackHasCooledDown && isInAttackDistance;
            bool isAttacking = _nextAttack.isActive;

            navMeshAgent.isStopped = isInAttackDistance;
            
            if (!isAttacking && canAttack)
            {
                _isAttacking = true;
                _nextAttack.OnStartAttack(_target);
            }
            else if (_isAttacking && attackHasCooledDown)
            {
                _nextAttack.OnUpdate();
                
            }
            else if (_isAttacking)
            {
                _isAttacking = false;
                enemy.SetMaterial(material);
            }
            else
            {
                HandlePositionAndRotation(isInAttackDistance, hasReachedDestination);
            }
        }

        private void HandlePositionAndRotation(bool isInAttackDistance, bool hasReachedDestination)
        {
            if (isInAttackDistance && !_nextAttack.isActive)
            {
                var enemyTransform = enemy.transform;
                var position = enemyTransform.position;
                var lookDirection = _target.Center() - position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
                return;
            }
            
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = _target.Center();
            if (hasReachedDestination)
            {
                navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceStay;
            }
            else
            {
                navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceFollow;
            }
        }
        
        private Attack NextAttack()
        {
            var attack = attacks[0];
            navMeshAgent.stoppingDistance = attack.stoppingDistanceFollow;
            return attack;
        }

        override
        public void OnExit()
        {
            aggressiveVision.gameObject.SetActive(false);
            aggressiveSensor.gameObject.SetActive(false);
            defaultVision.gameObject.SetActive(true);

            _target = null;
        }
        
        private Hittable GetTargetFromSensors()
        {
            if (defaultVision.canTargetBeDetected)
            {
                return defaultVision.target;
            }
            if (aggressiveVision.canTargetBeDetected)
            {
                return aggressiveVision.target;
            }
            if (aggressiveSensor.triggerCount > 0)
            {
                return  aggressiveSensor.target;
            }

            return null;
        }
    }
}
