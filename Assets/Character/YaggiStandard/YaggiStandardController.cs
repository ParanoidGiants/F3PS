using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum YaggiStandardStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

public enum YaggiStandardState
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

public enum YaggiStandardAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    EXECUTION,
    RECOVERY
}

public class YaggiStandardController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;

    public bool debugIsStopped;
    public float debugStoppingDistance;
    public Transform navMeshDestination;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public PatrolManager patrolManager;
    public TimeObject timeObject;
    public AnimateMesh animateMesh;
    public SensorController sensorController;
    public GameObject hittableParent;
    public Collider attackHitBox;
    public Transform uiHealthBarAnchor;

    [Space(20)]
    [Header("Settings")]
    public int health;
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
    [Header("Checking Settings")]
    public float checkingMoveSpeed = 0f;
    public Vector3 checkingDestination = Vector3.zero;

    [Space(10)]
    [Header("Suspicious Settings")]
    public float suspiciousTime;
    public float suspiciousDuration = 2f;
    public float suspiciousRotateSpeed = 30f;
    private Quaternion _startRotation;

    [Space(10)]
    [Header("Hit Settings")]
    public float hitTime = 0f;
    public float stunDuration = 0f;

    [Space(10)]
    [Header("Dying Settings")]
    public bool fadingOut = false;
    public float dieTime = 0f;
    public float fadeOutDuration = 1f;

    [Space(10)]
    [Header("Aggressive Settings")]
    public YaggiStandardStoppingDistanceState stoppingDistanceState = YaggiStandardStoppingDistanceState.STAYING;
    public float aggressiveMoveSpeed = 0f;
    public float aggressiveRotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;
    public Vector3 lastTargetPosition = Vector3.zero;

    [Header("Attack")]
    public Vector3 _attackForward;
    public YaggiStandardAttackState attackState = YaggiStandardAttackState.NONE;
    public float coolDownTime;
    public float coolDownDuration;
    public bool hitTargetEarly = false;

    [Header("Attack Anticipation")]
    public float attackAnticipationTime = 0f;
    public float attackAnticipationDuration = 1f;
    public float attackAnticipationRotationSpeed = 100f;

    [Header("Attack Execution")]
    public float attackExecutionTime = 0f;
    public float attackExecutionDuration = 1f;
    public float attackExecutionDistance = 0f;
    public Vector3 attackExecutionStartPosition;
    public Vector3 attackExecutionEndPosition;

    [Header("Attack Recovery")]
    public float attackRecoveryTime = 0f;
    public float attackRecoveryDuration = 1f;

    [Space(20)]
    [Header("Watchers")]
    public EnemyHealthUIPool _healthUIPool;
    public YaggiStandardState currentState = YaggiStandardState.IDLE;
    public bool isDead = false;
    private Vector3 lastPosition;

    protected void Awake()
    {
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();
        EnterState(YaggiStandardState.IDLE);
        _healthUIPool.CreateEnemyHealthUI(uiHealthBarAnchor);
    }

    private void SwitchState(YaggiStandardState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(YaggiStandardState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case YaggiStandardState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0);
                break;
            case YaggiStandardState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiStandardState.AGGRESSIVE:
                stoppingDistanceState = YaggiStandardStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = true;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = aggressiveMoveSpeed * TimeScale;
                break;
            case YaggiStandardState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiStandardState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 0f);
                break;
            case YaggiStandardState.RETURN_TO_IDLE:
                break;
            case YaggiStandardState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case YaggiStandardState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(YaggiStandardState currentState)
    {
        switch (currentState)
        {
            case YaggiStandardState.IDLE:
                break;
            case YaggiStandardState.PATROLLING:
                break;
            case YaggiStandardState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 1000;
                break;
            case YaggiStandardState.CHECKING:
                break;
            case YaggiStandardState.SUSPICIOUS:
                break;
            case YaggiStandardState.RETURN_TO_IDLE:
                break;
            case YaggiStandardState.HIT:
                break;
            case YaggiStandardState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }


    private void FixedUpdate()
    {
        var color = Color.white;
        switch (stoppingDistanceState)
        {
            case YaggiStandardStoppingDistanceState.PUSHED:
                color = Color.red;
                break;
            case YaggiStandardStoppingDistanceState.FOLLOWING:
                color = Color.yellow;
                break;
            case YaggiStandardStoppingDistanceState.STAYING:
                color = Color.green;
                break;
        }
        Debug.DrawLine(transform.position, navMeshAgent.destination, color, 4f);
        debugIsStopped = navMeshAgent.isStopped;
        debugStoppingDistance = navMeshAgent.stoppingDistance;


        if (currentState != YaggiStandardState.DYING && currentState != YaggiStandardState.AGGRESSIVE && sensorController.HasTarget())
        {
            SwitchState(YaggiStandardState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case YaggiStandardState.IDLE:
                if (idleDuration < 0f || patrolManager.PatrolPointCount <= 0)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(YaggiStandardState.PATROLLING);
                break;

            case YaggiStandardState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(YaggiStandardState.IDLE);
                break;

            case YaggiStandardState.AGGRESSIVE:
                if (attackState != YaggiStandardAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                if (!sensorController.HasTarget())
                {
                    SwitchState(YaggiStandardState.CHECKING);
                    return;
                }
                lastTargetPosition = sensorController.GetTargetFromSensors().Center();
                checkingDestination = lastTargetPosition;

                HandleAggressiveStoppingDistance(lastTargetPosition);

                var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, lastTargetPosition);
                var canAttack = coolDownTime >= coolDownDuration && distanceToTarget <= attackExecutionDistance;
                if (!canAttack)
                {
                    coolDownTime += ScaledDeltaTime;
                    break;
                }

                var targetDirection = (lastTargetPosition - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);
                if (isAlignedWithTarget)
                {
                    attackState = YaggiStandardAttackState.INIT;
                }

                break;
            case YaggiStandardState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(YaggiStandardState.SUSPICIOUS);
                }
                break;
            case YaggiStandardState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(YaggiStandardState.IDLE);
                break;
            case YaggiStandardState.RETURN_TO_IDLE:
                break;
            case YaggiStandardState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(YaggiStandardState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case YaggiStandardState.DYING:
                if (dieTime >= 0f)
                {
                    dieTime -= ScaledDeltaTime;
                    return;
                }

                if (!fadingOut)
                {
                    fadingOut = true;
                    animateMesh.FadeOut(fadeOutDuration);
                }

                if (fadeOutDuration >= 0f)
                {
                    fadeOutDuration -= ScaledDeltaTime;
                    return;
                }
                Debug.Log("Enemy Dead");
                Died();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void HandleAttackProcedure()
    {
        switch (attackState)
        {
            case YaggiStandardAttackState.INIT:
                attackState = YaggiStandardAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                hitTargetEarly = false;
                attackAnticipationTime = 0f;
                attackExecutionTime = 0f;
                attackRecoveryTime = 0f;
                animator.SetTrigger("Charge");
                break;
            case YaggiStandardAttackState.ANTICIPATION:
                var targetPosition = lastTargetPosition;
                if (sensorController.HasTarget())
                {
                    targetPosition = sensorController.GetTargetFromSensors().Center();
                    lastTargetPosition = targetPosition;
                }
                var lookDirection = targetPosition - transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, transform.up);
                var newRotation = Quaternion.LookRotation(newForward, transform.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    newRotation,
                    ScaledDeltaTime * attackAnticipationRotationSpeed
                );

                attackAnticipationTime += ScaledDeltaTime;
                if (attackAnticipationTime >= attackAnticipationDuration)
                {
                    _attackForward = transform.forward;
                    attackState = YaggiStandardAttackState.EXECUTION;
                    attackHitBox.enabled = true;
                    attackExecutionStartPosition = transform.position;
                    attackExecutionEndPosition = attackExecutionStartPosition + _attackForward * attackExecutionDistance;
                    MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", transform);
                    animator.SetTrigger("Attack");
                }
                break;
            case YaggiStandardAttackState.EXECUTION:
                transform.position = Vector3.Lerp(
                    attackExecutionStartPosition,
                    attackExecutionEndPosition,
                    attackExecutionTime / attackExecutionDuration
                );

                attackExecutionTime += ScaledDeltaTime;
                if (hitTargetEarly || attackExecutionTime >= attackExecutionDuration)
                {
                    attackState = YaggiStandardAttackState.RECOVERY;
                    attackHitBox.enabled = false;
                    animator.SetTrigger("Recover");
                }
                break;
            case YaggiStandardAttackState.RECOVERY:
                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryDuration)
                {
                    attackState = YaggiStandardAttackState.NONE;
                    coolDownTime = 0f;
                    SetupStoppingDistanceState(stoppingDistanceState, navMeshAgent.destination);
                }
                break;
            case YaggiStandardAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }


    private void HandleAggressiveStoppingDistance(Vector3 targetPosition)
    {
        var selectedTarget = sensorController.GetTargetFromSensors();
        var toTarget = selectedTarget.Center() - transform.position;
        var lookDirection = Vector3.ProjectOnPlane(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(lookDirection),
            aggressiveRotationSpeed * ScaledDeltaTime
        );


        if (!sensorController.IsTargetInLineOfSight())
        {
            navMeshAgent.stoppingDistance = 0;
            return;
        }

        if (selectedTarget == null)
        {
            return;
        }

        var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, selectedTarget.Center());
        if (stoppingDistanceState == YaggiStandardStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            SetupStoppingDistanceState(YaggiStandardStoppingDistanceState.FOLLOWING, selectedTarget.Center());
        }
        else if (stoppingDistanceState == YaggiStandardStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            SetupStoppingDistanceState(YaggiStandardStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaggiStandardStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == YaggiStandardStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            SetupStoppingDistanceState(YaggiStandardStoppingDistanceState.STAYING, selectedTarget.Center());
        }
        else if (stoppingDistanceState == YaggiStandardStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            SetupStoppingDistanceState(YaggiStandardStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaggiStandardStoppingDistanceState.FOLLOWING)
        {
            SetupStoppingDistanceState(YaggiStandardStoppingDistanceState.FOLLOWING, selectedTarget.Center());
        }
    }

    private void SetupStoppingDistanceState(YaggiStandardStoppingDistanceState state, Vector3 destination)
    {
        switch (state)
        {
            case YaggiStandardStoppingDistanceState.PUSHED:
                navMeshAgent.isStopped = false;
                navMeshAgent.stoppingDistance = 0;
                animator.SetFloat("Speed", -1f);
                break;
            case YaggiStandardStoppingDistanceState.STAYING:
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case YaggiStandardStoppingDistanceState.FOLLOWING:
                navMeshAgent.stoppingDistance = 0;
                navMeshAgent.isStopped = false;
                animator.SetFloat("Speed", 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        stoppingDistanceState = state;
        SetDestination(destination);
    }

    private void SetDestination(Vector3 position)
    {
        navMeshDestination.position = position;
        navMeshAgent.destination = position;
    }

    public virtual void Hit(int damage)
    {
        if (currentState is YaggiStandardState.DYING)
        {
            return;
        }

        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.RemoveEnemyHealthUI(uiHealthBarAnchor);
            SwitchState(YaggiStandardState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(uiHealthBarAnchor, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(YaggiStandardState state)
    {
        if (state == YaggiStandardState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is YaggiStandardState.CHECKING or YaggiStandardState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not YaggiStandardState.DYING)
        {
            sensorController.SetState(SensorState.IDLE);
        }
    }

    public void Deactivate()
    {
        navMeshAgent.enabled = false;
        hittableParent.SetActive(false);
    }

    public void Died()
    {
        Destroy(gameObject);
    }

    public void HitByPlayerFrom(Vector3 hitDirection)
    {
        if (currentState is YaggiStandardState.DYING or YaggiStandardState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(YaggiStandardState.CHECKING);
    }
}
