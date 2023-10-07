using System;
using F3PS.AI.States.Action;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.States
{
    public class Aggressive : State
    {
        private Attack _nextAttack;
        private Attack[] _attacks;

        [Space(10)]
        [Header("Specific Watchers")]
        [SerializeField] private Hittable _selectedTarget;

        private void Awake()
        {
            _attacks = GetComponentsInChildren<Attack>();
        }

        private void Start()
        {
            foreach (var attack in _attacks)
            {
                attack.Init();
            }
        }

        private void Update()
        {
            foreach (var attack in _attacks)
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
            HandlePositionAndRotation();
            HandleStoppingDistance();
        }
        
        override
        public void OnExit()
        {
            base.OnExit();
        }

        public bool IsTargetDetected()
        {
            return stateManager.sensorController.IsTargetDetected();
        }

        override
        public void OnUpdate()
        {
            bool isAttacking = _nextAttack.isActive;
            bool isTargetDetected = IsTargetDetected();
            
            if (isTargetDetected)
            {
                _navMeshAgent.destination = _selectedTarget.Center();
            }
            _navMeshAgent.isStopped = isAttacking;
            
            if (!isAttacking && !isTargetDetected)
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }


            
            if (isAttacking)
            {
                _nextAttack.OnUpdate();
                return;
            }
            
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            bool attackHasCooledDown = _nextAttack.HasCooledDown();
            if (attackHasCooledDown && _nextAttack.IsInAttackDistance(_selectedTarget.Center()))
            {
                _nextAttack.OnStartAttack(_selectedTarget);
            }
            else
            {
                HandlePositionAndRotation();
            }
            HandleStoppingDistance();
        }

        private void HandlePositionAndRotation()
        {
            bool isInAttackDistance = _nextAttack.IsInAttackDistance(_selectedTarget.Center());
            if (isInAttackDistance)
            {
                var enemyTransform = enemy.body.transform;
                var position = enemyTransform.position;
                var lookDirection = _selectedTarget.Center() - position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
            }
        }

        private void HandleStoppingDistance()
        {
            if (Helper.HasReachedDestination(_navMeshAgent))
            {
                _navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceStay;
            }
            else
            {
                _navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceFollow;
            }
        }
        
        private Attack NextAttack()
        {
            var attack = _attacks[0];
            _navMeshAgent.stoppingDistance = attack.stoppingDistanceFollow;
            return attack;
        }
    }
}
