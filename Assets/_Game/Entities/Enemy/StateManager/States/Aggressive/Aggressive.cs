using F3PS.AI.States.Action;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.States
{
    public class Aggressive : State
    {
        private Attack _nextAttack;
        
        [Space(10)]
        [Header("Specific Settings")]
        public Attack[] attacks;

        [Space(10)]
        [Header("Specific Watchers")]
        [SerializeField] private Hittable _selectedTarget;

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
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            _nextAttack = NextAttack();
            HandlePositionAndRotation(false);
        }


        public bool IsTargetDetected()
        {
            return stateManager.sensorController.IsTargetDetected();
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
            else if (isAttacking)
            {
                _nextAttack.OnUpdate();
                return;
            }
            
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            bool attackHasCooledDown = _nextAttack.HasCooledDown();
            bool isInAttackDistance = _nextAttack.IsInAttackDistance(_selectedTarget.Center());

            if (attackHasCooledDown && isInAttackDistance)
            {
                _nextAttack.OnStartAttack(_selectedTarget);
            }
            else
            {
                HandlePositionAndRotation(isInAttackDistance);
            }
        }

        private void HandlePositionAndRotation(bool isInAttackDistance)
        {
            bool hasReachedDestination = Helper.HasReachedDestination(navMeshAgent);
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
    }
}
