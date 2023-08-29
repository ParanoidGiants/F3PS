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
            if (Helper.HasReachedDestination(navMeshAgent))
            {
                navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceStay;
            }
            else
            {
                navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceFollow;
            }
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
                navMeshAgent.destination = _selectedTarget.Center();
            }
            navMeshAgent.isStopped = isAttacking;
            
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
            bool isInAttackDistance = _nextAttack.IsInAttackDistance(_selectedTarget.Center());

            if (attackHasCooledDown && isInAttackDistance)
            {
                _nextAttack.OnStartAttack(_selectedTarget);
            }
            else
            {
                if (isInAttackDistance)
                {
                    var enemyTransform = enemy.transform;
                    var position = enemyTransform.position;
                    var lookDirection = _selectedTarget.Center() - position;
                    var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                    var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                    enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, Time.deltaTime * 5f);
                }
            
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceStay;
                }
                else
                {
                    navMeshAgent.stoppingDistance = _nextAttack.stoppingDistanceFollow;
                }
            }
        }

        private void HandlePositionAndRotation(bool isInAttackDistance)
        {
        }
        
        private Attack NextAttack()
        {
            var attack = _attacks[0];
            navMeshAgent.stoppingDistance = attack.stoppingDistanceFollow;
            return attack;
        }
    }
}
