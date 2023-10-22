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
        [Header("Specific Settings")]
        public float rotationSpeed;
        
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
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            _nextAttack = NextAttack();
            HandlePositionAndRotation();
            HandleStoppingDistance();
        }
        
        public bool IsTargetDetected()
        {
            return stateManager.sensorController.IsTargetDetected();
        }

        override
        public void OnUpdate()
        {
            bool isAttacking = _nextAttack.isActive;
            _navMeshAgent.isStopped = _nextAttack.isAttacking;
            if (isAttacking)
            {
                _nextAttack.OnUpdate();
                return;
            }
            
            if (!IsTargetDetected())
            {
                stateManager.SwitchState(StateType.CHECKING);
                return;
            }
            HandlePositionAndRotation();
            HandleStoppingDistance();
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
            
            _selectedTarget = stateManager.sensorController.GetTargetFromSensors();
            _navMeshAgent.destination = _selectedTarget.Center();
            bool attackHasCooledDown = _nextAttack.HasCooledDown();
            if (attackHasCooledDown && Helper.HasReachedDestination(_navMeshAgent))
            {
                _nextAttack.OnStartAttack(_selectedTarget);
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
