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
            }
        }
        
        
        public void HandlePositioning()
        {
            if (!IsTargetDetected()) return;
            
            navMeshAgent.destination = _target.position;
        
            if (Helper.HasReachedDestination(navMeshAgent) && nextAttack.HasCooledDown())
            {
                _isAttacking = true;
                nextAttack.OnStartAttack(_target);
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
