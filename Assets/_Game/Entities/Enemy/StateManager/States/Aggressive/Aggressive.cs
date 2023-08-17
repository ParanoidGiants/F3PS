using UnityEngine;

namespace Enemy.States
{
    public class Aggressive : State
    {
        private bool _isAttacking;
        private Hittable _target;
        
        public Vision aggressiveVision;
        public AggressiveSensor aggressiveSensor;
        public Attack[] attacks;
        public Attack nextAttack;
        
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
            
            nextAttack = NextAttack();
            HandlePositionAndRotation(false, false);
        }

        private void HandlePositionAndRotation(bool isInAttackDistance, bool hasReachedDestination)
        {
            if (isInAttackDistance)
            {
                navMeshAgent.isStopped = true;
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
                navMeshAgent.stoppingDistance = nextAttack.stoppingDistanceStay;
            }
            else
            {
                navMeshAgent.stoppingDistance = nextAttack.stoppingDistanceFollow;
            }
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
            bool attackHasCooledDown = nextAttack.HasCooledDown();
            bool isInAttackDistance = nextAttack.IsInAttackDistance(_target.transform.position);
            bool canAttack = attackHasCooledDown && isInAttackDistance;
            bool isAttacking = nextAttack.isActive;

            if (!isAttacking && canAttack)
            {
                _isAttacking = true;
                nextAttack.OnStartAttack(_target);
            }
            else if (_isAttacking && attackHasCooledDown)
            {
                nextAttack.OnUpdate();
                
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
