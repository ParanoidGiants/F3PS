using F3PS.AI.States.Action;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.States
{
    public class Aggressive : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        public float rotationSpeed;
        
        [Space(10)]
        [Header("Specific Watchers")]
        [SerializeField] private Hittable _selectedTarget;
        [SerializeField] private bool _isStaying;
        [SerializeField] private Attack _currentAttack;
        [SerializeField] private Attack[] _attacks;

        override
        public void Initialize()
        {
            base.Initialize();
            _attacks = GetComponentsInChildren<Attack>();
            foreach (var attack in _attacks)
            {
                attack.Initialize(material);
            }
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            _navMeshAgent.isStopped = false;
            _currentAttack = NextAttack();
            HandleStoppingDistance();
        }

        override
        public void OnPhysicsUpdate()
        {
            bool hasTarget = stateManager.sensorController.IsTargetDetected();
            if (hasTarget)
            {
                _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
                _navMeshAgent.destination = _selectedTarget.Center();
                HandleStoppingDistance();
            }
            
            if (_currentAttack.isActive)
            {
                _currentAttack.OnUpdate();
                return;
            }
            
            if (!hasTarget)
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }
            
            if (!_currentAttack.isActive
                && _isStaying
                && _currentAttack.CanAttack(_selectedTarget.Center())
            ) {
                _currentAttack.OnStartAttack(_selectedTarget);
                return;
            }
            HandlePositionAndRotation();
        }

        override
        public void OnFrameUpdate()
        {
            foreach (var attack in _attacks)
            {
                if (attack.HasCooledDown())
                    continue;
                attack.CoolDown();
            }
        }
        
        private void HandlePositionAndRotation()
        {
            bool isInAttackDistance = Helper.HasReachedDestination(_navMeshAgent);
            if (isInAttackDistance)
            {
                var enemyTransform = enemy.body.transform;
                var position = enemyTransform.position;
                var lookDirection = _selectedTarget.Center() - position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
                var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
                enemyTransform.rotation = Quaternion.RotateTowards(
                    enemyTransform.rotation,
                    newRotation,
                    enemy.ScaledDeltaTime * rotationSpeed
                );
            }
        }

        private void HandleStoppingDistance()
        {
            _isStaying = Helper.HasReachedDestination(_navMeshAgent);
            if (_isStaying)
            {
                _navMeshAgent.stoppingDistance = _currentAttack.stoppingDistanceFollow;
            }
            else
            {
                _navMeshAgent.stoppingDistance = _currentAttack.stoppingDistanceStay;
            }
        }
        
        private Attack NextAttack()
        {
            var attack = _attacks[0];
            _navMeshAgent.stoppingDistance = attack.stoppingDistanceStay;
            return attack;
        }
    }
}
