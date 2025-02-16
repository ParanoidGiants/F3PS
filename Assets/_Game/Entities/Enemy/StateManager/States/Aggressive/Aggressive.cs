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
        [SerializeField] private bool _isStaying;
        [SerializeField] private Hittable _selectedTarget;
        [SerializeField] private Attack _currentAttack;
        [SerializeField] private Attack[] _attacks;
        
        [SerializeField] private bool _isAttacking;
        public bool IsAttacking => _isAttacking;


        override
        public void Initialize()
        {
            base.Initialize();
            _attacks = GetComponentsInChildren<Attack>();
            foreach (var attack in _attacks)
            {
                attack.Initialize(material);
            }
            _currentAttack = _attacks[0];
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            _navMeshAgent.isStopped = false;
            ChangeAttack(AttackType.RUSH);
            HandleStoppingDistance();
        }
        
        override 
        public void OnExit()
        {
            base.OnExit();
            _navMeshAgent.isStopped = true;
        }

        override
        public void OnPhysicsUpdate()
        {
            _isAttacking = _currentAttack.isActive;
            if (_currentAttack.isActive)
            {
                _currentAttack.OnPhysicsUpdate();
                return;
            }
            
            bool hasTarget = stateManager.sensorController.IsTargetDetected();
            if (!hasTarget)
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }

            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            _navMeshAgent.destination = _selectedTarget.Center();
            HandleStoppingDistance();
            
            if (_isStaying && _currentAttack.CanAttack(_selectedTarget))
            {
                _currentAttack.OnStartAttack(_selectedTarget);
            }
        }

        override
        public void OnFrameUpdate()
        {
            foreach (var attack in _attacks)
            {
                attack.OnFrameUpdate();
            }
            
            if (_currentAttack.isActive
                || !stateManager.sensorController.IsTargetDetected()
                || !_selectedTarget
            )
                return;
            
            HandlePositionAndRotation();
        }
        
        private void HandlePositionAndRotation()
        {
            bool isInAttackDistance = Helper.HasReachedDestination(_navMeshAgent);
            if (!isInAttackDistance)
            {
                return;
            }
            var enemyTransform = enemy.body.transform;
            var lookDirection = _selectedTarget.Center() - enemyTransform.position;
            var newForward = Vector3.ProjectOnPlane(lookDirection, enemyTransform.up);
            var newRotation = Quaternion.LookRotation(newForward, enemyTransform.up);
            enemyTransform.rotation = Quaternion.RotateTowards(
                enemyTransform.rotation,
                newRotation,
                enemy.ScaledDeltaTime * rotationSpeed
            );
        }

        private void HandleStoppingDistance()
        {
            _isStaying = Helper.HasReachedDestination(_navMeshAgent);
            if (!stateManager.sensorController.IsTargetInLineOfSight())
            {
                _navMeshAgent.stoppingDistance = 0;
            }
            else if (_isStaying)
            {
                _navMeshAgent.stoppingDistance = _currentAttack.stoppingDistanceStay;
            }
            else
            {
                _navMeshAgent.stoppingDistance = _currentAttack.stoppingDistanceFollow;
            }
        }
        
        public void ChangeAttack(AttackType attackType)
        {
            foreach (var attack in _attacks)
            {
                if (attack.type == attackType)
                {
                    _currentAttack = attack;
                    _navMeshAgent.stoppingDistance = attack.stoppingDistanceStay;
                    return;
                }
            }
        }
    }
}
