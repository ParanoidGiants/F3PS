using UnityEngine;

namespace Enemy.States
{
    public class Aggressive : State
    {
        private Hittable _selectedTarget;
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
            _selectedTarget = GetTargetFromSensors();
            // set default vision to false AFTER getting the target from the sensors
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
            bool isAttacking = _nextAttack.isActive;
            if (!isAttacking && !IsTargetDetected())
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }
            bool hasReachedDestination = Helper.HasReachedDestination(navMeshAgent);
            bool attackHasCooledDown = _nextAttack.HasCooledDown();
            bool isInAttackDistance = _nextAttack.IsInAttackDistance(_selectedTarget.Center());
            bool canAttack = attackHasCooledDown && isInAttackDistance;

            if (!isAttacking && canAttack)
            {
                _nextAttack.OnStartAttack(_selectedTarget);
            }
            else if (isAttacking)
            {
                _nextAttack.OnUpdate();
            }
            else
            {
                HandlePositionAndRotation(isInAttackDistance, hasReachedDestination);
            }
        }

        private void HandlePositionAndRotation(bool isInAttackDistance, bool hasReachedDestination)
        {
            navMeshAgent.isStopped = isInAttackDistance;
            navMeshAgent.destination = _selectedTarget.Center();
            
            if (isInAttackDistance)
            {
                var enemyTransform = enemy.transform;
                var position = enemyTransform.position;
                var lookDirection = _selectedTarget.Center() - position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
            }
            
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
            _selectedTarget = null;
        }
        
        private Hittable GetTargetFromSensors()
        {
            if (defaultVision.canTargetBeDetected)
            {
                return defaultVision.SelectedTarget;
            }
            if (aggressiveVision.canTargetBeDetected)
            {
                return aggressiveVision.SelectedTarget;
            }
            if (aggressiveSensor.triggerCount > 0)
            {
                return  aggressiveSensor.target;
            }

            return null;
        }
    }
}
