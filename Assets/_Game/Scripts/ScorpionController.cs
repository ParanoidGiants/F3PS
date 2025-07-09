using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Damage.Take;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum ScorpionState
{
    IDLE,
    AGGRESSIVE,
    CHECKING,
    SUSPICIOUS,
    RETURN_TO_IDLE,
    PATROLLING,
    HIT,
    DYING
}

public enum ScorpionAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    EXECUTION,
    RECOVERY
}

public class ScorpionController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navMeshAgent; 
    public Hittable[] _hittables;
    public PatrolManager patrolManager;
    public TimeObject timeObject;
    public AnimateMesh animateMesh;
    public SensorController sensorController;
    public GameObject chargeFlare;
    public Collider attackHitBox;

    [Space(20)]
    [Header("Settings")]
    public int maxHealth = 100;
    public float moveSpeed;

    [Space(10)]
    [Header("Idle Settings")]
    public float idleDuration = 0f;
    public float idleTime = 0f;

    [Space(10)]
    [Header("Patrol Settings")]
    public float patrolMoveSpeed = 0f;

    [Space(10)]
    [Header("Aggressive Settings")]
    public Hittable _selectedTarget;
    public float rotationSpeed;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;
    [Header("Attack")]
    public ScorpionAttackState attackState = ScorpionAttackState.NONE;
    public bool isStaying = false;
    public float coolDownTime;
    public float coolDownDuration;
    public bool hitTargetEarly = false;
    public Vector3 _attackForward;

    [Header("Attack Anticipation")]
    public float attackAnticipationTime = 0f;
    public float attackAnticipationDuration = 1f;
    public float attackAnticipationDistance = 0f;
    public Vector3 attackAnticipationStartPosition;
    public Vector3 attackAnticipationEndPosition;

    [Header("Attack Execution")]
    public float attackExecutionTime = 0f;
    public float attackExecutionDuration = 1f;
    public float attackExecutionDistance = 0f;
    public Vector3 attackExecutionStartPosition;
    public Vector3 attackExecutionEndPosition;

    [Header("Attack Recovery")]
    public float attackRecoveryTime = 0f;
    public float attackRecoveryDuration = 1f;
    public float attackRecoveryDistance = 0f;
    public Vector3 attackRecoveryStartPosition;
    public Vector3 attackRecoveryEndPosition;

    [Space(20)]
    [Header("Watchers")]
    public EnemyHealthUIPool _healthUIPool;
    public ScorpionState currentState = ScorpionState.IDLE;
    public bool isActive = true;
    public bool isDying = false;
    public bool isDead = false;
    public int health;

    protected void Awake()
    {
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
    }

    private void Start()
    {
        if (!isActive) return;

        health = maxHealth;
        patrolManager.Init();
        EnterState(ScorpionState.IDLE);
    }

    private void SwitchState(ScorpionState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(ScorpionState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case ScorpionState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case ScorpionState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case ScorpionState.AGGRESSIVE:
                navMeshAgent.isStopped = false;
                HandleAggressiveStoppingDistance();
                animator.SetFloat("Speed", 1f);
                break;
            case ScorpionState.CHECKING:
                break;
            case ScorpionState.SUSPICIOUS:
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.HIT:
                break;
            case ScorpionState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(ScorpionState currentState)
    {
        switch (currentState)
        {
            case ScorpionState.IDLE:
                break;
            case ScorpionState.PATROLLING:
                break;
            case ScorpionState.AGGRESSIVE:
                break;
            case ScorpionState.CHECKING:
                break;
            case ScorpionState.SUSPICIOUS:
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.HIT:
                break;
            case ScorpionState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void FixedUpdate()
    {
        if (!isActive || currentState == ScorpionState.DYING)
        {
            return;
        }

        if (currentState != ScorpionState.AGGRESSIVE && sensorController.IsTargetDetected())
        {
            SwitchState(ScorpionState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case ScorpionState.IDLE:
                if (idleDuration < 0f)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(ScorpionState.PATROLLING);
                break;

            case ScorpionState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(ScorpionState.IDLE);
                break;

            case ScorpionState.AGGRESSIVE:
                if (attackState != ScorpionAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                bool hasTarget = sensorController.IsTargetDetected();
                if (!hasTarget)
                {
                    SwitchState(ScorpionState.CHECKING);
                    return;
                }

                _selectedTarget = sensorController.GetTargetFromSensors();
                navMeshAgent.destination = _selectedTarget.Center();
                HandleAggressiveStoppingDistance();

                var canAttack = isStaying && coolDownTime >= coolDownDuration;
                if (!canAttack)
                {
                    break;
                }

                var targetDirection = (_selectedTarget.Center() - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);

                if (isAlignedWithTarget)
                {
                    attackState = ScorpionAttackState.INIT;
                }

                break;
            case ScorpionState.CHECKING:
                break;
            case ScorpionState.SUSPICIOUS:
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.HIT:
                break;
            case ScorpionState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void HandleAttackProcedure()
    {
        switch (attackState)
        {
            case ScorpionAttackState.INIT:
                attackState = ScorpionAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                hitTargetEarly = false;
                attackAnticipationTime = 0f;
                attackExecutionTime = 0f;
                attackRecoveryTime = 0f;

                _attackForward = transform.forward;

                attackAnticipationStartPosition = transform.position;
                attackAnticipationEndPosition = attackAnticipationStartPosition - _attackForward * attackAnticipationDistance;

                chargeFlare.SetActive(true);
                animator.SetTrigger("Charge");
                break;
            case ScorpionAttackState.ANTICIPATION:
                var lookDirection = _selectedTarget.Center() - transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, transform.up);
                var newRotation = Quaternion.LookRotation(newForward, transform.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    newRotation,
                    ScaledDeltaTime * 80
                );
                transform.position = Vector3.Lerp(
                    attackAnticipationStartPosition,
                    attackAnticipationEndPosition,
                    attackAnticipationTime / attackAnticipationDuration
                );

                attackAnticipationTime += ScaledDeltaTime;
                if (attackAnticipationTime >= attackAnticipationDuration)
                {
                    attackState = ScorpionAttackState.EXECUTION;
                    attackHitBox.enabled = true;
                    attackExecutionStartPosition = transform.position;
                    attackExecutionEndPosition = attackExecutionStartPosition + _attackForward * attackExecutionDistance;
                    MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", transform);
                    animator.SetTrigger("Attack");
                }
                break;
            case ScorpionAttackState.EXECUTION:
                transform.position = Vector3.Lerp(
                    attackExecutionStartPosition,
                    attackExecutionEndPosition,
                    attackExecutionTime / attackExecutionDuration
                );

                attackExecutionTime += ScaledDeltaTime;
                if (hitTargetEarly || attackExecutionTime >= attackExecutionDuration)
                {
                    attackState = ScorpionAttackState.RECOVERY;
                    attackHitBox.enabled = false;
                    attackRecoveryStartPosition = transform.position;
                    attackRecoveryEndPosition = attackRecoveryStartPosition - _attackForward * attackRecoveryDistance;
                    animator.SetTrigger("Recover");
                }
                break;
            case ScorpionAttackState.RECOVERY:
                transform.position = Vector3.Lerp(
                    attackRecoveryStartPosition,
                    attackRecoveryEndPosition,
                    attackRecoveryTime / attackRecoveryDuration
                );

                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryDuration)
                {
                    attackState = ScorpionAttackState.NONE;
                    coolDownTime = 0f;
                    navMeshAgent.isStopped = false;
                }
                break;
            case ScorpionAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }

    private void Update()
    {
        if (!isActive) return;

        switch (currentState)
        {
            case ScorpionState.IDLE:
                break;
            case ScorpionState.AGGRESSIVE:
                break;
            case ScorpionState.CHECKING:
                break;
            case ScorpionState.SUSPICIOUS:
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.PATROLLING:
                break;
            case ScorpionState.HIT:
                break;
            case ScorpionState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void HandleAggressiveStoppingDistance()
    {
        isStaying = Helper.HasReachedDestination(navMeshAgent);
        if (!sensorController.IsTargetInLineOfSight())
        {
            navMeshAgent.stoppingDistance = 0;
        }
        else if (isStaying)
        {
            navMeshAgent.stoppingDistance = stoppingDistanceStay;
        }
        else
        {
            navMeshAgent.stoppingDistance = stoppingDistanceFollow;
        }
    }

    public virtual void Hit(int damage)
    {
        if (isDying)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.OnKillTarget(transform);
            isDead = true;
            // _stateManager.SwitchState(StateType.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(transform, health, maxHealth);
        animateMesh.HitFlash();
        // _stateManager.Hit();
    }

    private void UpdateSensorState(ScorpionState state)
    {
        if (state == ScorpionState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is ScorpionState.CHECKING or ScorpionState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else
        {
            sensorController.SetState(SensorState.IDLE);
        }
    }

    public void Deactivate()
    {
        isActive = false;
        navMeshAgent.enabled = false;
        foreach (var hittable in _hittables)
        {
            hittable.enabled = false;
        }
    }

    public void Died()
    {
        Destroy(gameObject);
    }

    public void HitByPlayerFrom(Vector3 hitDirection)
    {
        navMeshAgent.destination = navMeshAgent.transform.position - hitDirection;
        SwitchState(ScorpionState.CHECKING);
    }
}
