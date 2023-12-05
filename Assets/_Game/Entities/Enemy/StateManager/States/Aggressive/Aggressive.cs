using F3PS.AI.States.Action;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.States
{
    public class Aggressive : State
    {
        private Attack _currentAttack;
        private Attack[] _attacks;
        public bool _isStaying;

        [Space(10)]
        [Header("Specific Settings")]
        public float rotationSpeed;
        
        [Space(10)]
        [Header("Specific Watchers")]
        [SerializeField] private Hittable _selectedTarget;

        private void Awake()
        {
            _attacks = GetComponentsInChildren<Attack>();
            foreach (var attack in _attacks)
            {
                attack.Init(material);
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
            _currentAttack = NextAttack();
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            if (_currentAttack.isActive)
            {
                _currentAttack.OnUpdate();
                return;
            }

            if (!stateManager.sensorController.IsTargetDetected())
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }
            HandleStoppingDistance();
            _isStaying = Helper.HasReachedDestination(_navMeshAgent);          
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            HandlePositionAndRotation();
            HandleNextAttack();
            
        }

        private void HandleNextAttack()
        {
            Transform transform1 = enemy.body.transform;
            var targetForward = (_selectedTarget.Center() - transform1.position).normalized;
            var actualForward = transform1.forward;
            if (
                !Helper.IsOrientedOnXZ(actualForward, targetForward, 0.1f)
                && !Helper.IsOnSameY(transform1.position, _selectedTarget.Center(), 0.1f)
            ) return;
            
            _navMeshAgent.destination = _selectedTarget.Center();
            if (
                _currentAttack.HasCooledDown() 
                && 
                _isStaying 
                && 
                _currentAttack.CanAttack(_selectedTarget.Center())
            ) {
                _currentAttack.OnStartAttack(_selectedTarget);
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
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, newRotation, enemy.ScaledDeltaTime * rotationSpeed);
            }
        }

        private void HandleStoppingDistance()
        {
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
