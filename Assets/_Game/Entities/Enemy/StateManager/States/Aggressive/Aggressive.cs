using UnityEngine;

namespace Enemy.States
{
    public class Aggressive : State
    {
        private bool _isAttacking;
        private Transform _target;
        
        public Vision aggressiveVision;
        public AggressiveSensor aggressiveSensor;
        public Attack[] attacks;
        public Attack nextAttack;
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            aggressiveVision.gameObject.SetActive(true);
            aggressiveSensor.gameObject.SetActive(true);
            _target = GetTargetFromSensors();
            defaultVision.gameObject.SetActive(false);

            nextAttack = NextAttack();
        }

        private Transform GetTargetFromSensors()
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

        public bool IsTargetDetected()
        {
            return defaultVision.canTargetBeDetected 
                   || aggressiveVision.canTargetBeDetected
                   || aggressiveVision.triggerCount > 0;
        }

        override
        public void OnUpdate()
        {
            if (!_isAttacking && !aggressiveSensor.isTargetDetected && !aggressiveVision.IsTargetInSight())
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }

            if (!_isAttacking)
            {
                HandlePositioning();
            }
            else if (nextAttack.HasCooledDown())
            {
                nextAttack.OnUpdate();
            }
            else
            {
                _isAttacking = false;
                enemy.SetMaterial(material);
            }
        }
        
        
        public void HandlePositioning()
        {
            if (!IsTargetDetected()) return;
            navMeshAgent.destination = _target.position;

            if (!Helper.HasReachedDestination(navMeshAgent)) return;
        
            if (nextAttack.HasCooledDown())
            {
                _isAttacking = true;
                nextAttack.OnStartAttack(_target);
            }
            else
            {
                var enemyTransform = enemy.transform;
                var position = enemyTransform.position;
                var lookDirection = _target.position - position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
                Debug.DrawRay(position, newForward, Color.red);
            }
        }

        private void Update()
        {
            foreach (var attack in attacks)
            {
                if (attack.HasCooledDown()) continue;
                attack.CoolDown();
            }
        }

        private Attack NextAttack()
        {
            var attack = attacks[0];
            navMeshAgent.stoppingDistance = attack.stoppingDistance;
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
    }
}
