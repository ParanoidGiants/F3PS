using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum FromSpawnerYaggiStandardStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

public enum FromSpawnerYaggiStandardState
{
    FALLING,
    IDLE,
    AGGRESSIVE,
    CHECKING,
    SUSPICIOUS,
    RETURN_TO_IDLE,
    PATROLLING,
    HIT,
    DYING
}

public enum FromSpawnerYaggiStandardAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    EXECUTION,
    RECOVERY
}

public class FromSpawnerYaggiStandardController : MonoBehaviour
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
    public FromSpawnerYaggiStandardStoppingDistanceState stoppingDistanceState = FromSpawnerYaggiStandardStoppingDistanceState.STAYING;
    public float aggressiveMoveSpeed = 0f;
    public float aggressiveRotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;
    public Vector3 lastTargetPosition = Vector3.zero;

    [Header("Attack")]
    public Vector3 _attackForward;
    public FromSpawnerYaggiStandardAttackState attackState = FromSpawnerYaggiStandardAttackState.NONE;
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
    public FromSpawnerYaggiStandardState currentState = FromSpawnerYaggiStandardState.IDLE;
    public bool isDead = false;
    public Rigidbody _rigidbody;

    protected void Awake()
    {
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();
        EnterState(FromSpawnerYaggiStandardState.FALLING);
        _healthUIPool.CreateEnemyHealthUI(uiHealthBarAnchor);
    }

    private void SwitchState(FromSpawnerYaggiStandardState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(FromSpawnerYaggiStandardState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case FromSpawnerYaggiStandardState.FALLING:
                _rigidbody.isKinematic = false;
                navMeshAgent.enabled = false;
                patrolManager.SetNextPatrolPointToClosestPoint(transform.position);
                break;
            case FromSpawnerYaggiStandardState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0);
                break;
            case FromSpawnerYaggiStandardState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case FromSpawnerYaggiStandardState.AGGRESSIVE:
                stoppingDistanceState = FromSpawnerYaggiStandardStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = true;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = aggressiveMoveSpeed * TimeScale;
                break;
            case FromSpawnerYaggiStandardState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case FromSpawnerYaggiStandardState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 0f);
                break;
            case FromSpawnerYaggiStandardState.RETURN_TO_IDLE:
                break;
            case FromSpawnerYaggiStandardState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case FromSpawnerYaggiStandardState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(FromSpawnerYaggiStandardState currentState)
    {
        switch (currentState)
        {
            case FromSpawnerYaggiStandardState.FALLING:
                _rigidbody.isKinematic = true;
                navMeshAgent.enabled = true;
                navMeshAgent.Warp(transform.position);
                break;
            case FromSpawnerYaggiStandardState.IDLE:
                break;
            case FromSpawnerYaggiStandardState.PATROLLING:
                break;
            case FromSpawnerYaggiStandardState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 1000;
                break;
            case FromSpawnerYaggiStandardState.CHECKING:
                break;
            case FromSpawnerYaggiStandardState.SUSPICIOUS:
                break;
            case FromSpawnerYaggiStandardState.RETURN_TO_IDLE:
                break;
            case FromSpawnerYaggiStandardState.HIT:
                break;
            case FromSpawnerYaggiStandardState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }


    private void FixedUpdate()
    {
        if (currentState == FromSpawnerYaggiStandardState.FALLING)
        {
            if (Physics.Raycast(transform.position, Vector3.down, 1f, Helper.GroundLayer))
            {
                SwitchState(FromSpawnerYaggiStandardState.IDLE);
            }
            return;
        }
        debugIsStopped = navMeshAgent.isStopped;
        debugStoppingDistance = navMeshAgent.stoppingDistance;


        if (currentState != FromSpawnerYaggiStandardState.DYING && currentState != FromSpawnerYaggiStandardState.AGGRESSIVE && sensorController.HasTarget())
        {
            SwitchState(FromSpawnerYaggiStandardState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case FromSpawnerYaggiStandardState.IDLE:
                if (idleDuration < 0f || patrolManager.PatrolPointCount <= 0)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(FromSpawnerYaggiStandardState.PATROLLING);
                break;

            case FromSpawnerYaggiStandardState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(FromSpawnerYaggiStandardState.IDLE);
                break;

            case FromSpawnerYaggiStandardState.AGGRESSIVE:
                if (attackState != FromSpawnerYaggiStandardAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                if (!sensorController.HasTarget())
                {
                    SwitchState(FromSpawnerYaggiStandardState.CHECKING);
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
                    attackState = FromSpawnerYaggiStandardAttackState.INIT;
                }

                break;
            case FromSpawnerYaggiStandardState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(FromSpawnerYaggiStandardState.SUSPICIOUS);
                }
                break;
            case FromSpawnerYaggiStandardState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(FromSpawnerYaggiStandardState.IDLE);
                break;
            case FromSpawnerYaggiStandardState.RETURN_TO_IDLE:
                break;
            case FromSpawnerYaggiStandardState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(FromSpawnerYaggiStandardState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case FromSpawnerYaggiStandardState.DYING:
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
            case FromSpawnerYaggiStandardAttackState.INIT:
                attackState = FromSpawnerYaggiStandardAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                hitTargetEarly = false;
                attackAnticipationTime = 0f;
                attackExecutionTime = 0f;
                attackRecoveryTime = 0f;
                animator.SetTrigger("Charge");
                break;
            case FromSpawnerYaggiStandardAttackState.ANTICIPATION:
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
                    attackState = FromSpawnerYaggiStandardAttackState.EXECUTION;
                    attackHitBox.enabled = true;
                    attackExecutionStartPosition = transform.position;
                    attackExecutionEndPosition = attackExecutionStartPosition + _attackForward * attackExecutionDistance;
                    MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", transform);
                    animator.SetTrigger("Attack");
                }
                break;
            case FromSpawnerYaggiStandardAttackState.EXECUTION:
                transform.position = Vector3.Lerp(
                    attackExecutionStartPosition,
                    attackExecutionEndPosition,
                    attackExecutionTime / attackExecutionDuration
                );

                attackExecutionTime += ScaledDeltaTime;
                if (hitTargetEarly || attackExecutionTime >= attackExecutionDuration)
                {
                    attackState = FromSpawnerYaggiStandardAttackState.RECOVERY;
                    attackHitBox.enabled = false;
                    animator.SetTrigger("Recover");
                }
                break;
            case FromSpawnerYaggiStandardAttackState.RECOVERY:
                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryDuration)
                {
                    attackState = FromSpawnerYaggiStandardAttackState.NONE;
                    coolDownTime = 0f;
                    SetupStoppingDistanceState(stoppingDistanceState, navMeshAgent.destination);
                }
                break;
            case FromSpawnerYaggiStandardAttackState.NONE:
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

        var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, targetPosition);
        if (stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState.FOLLOWING, targetPosition);
        }
        else if (stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState.STAYING, targetPosition);
        }
        else if (stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == FromSpawnerYaggiStandardStoppingDistanceState.FOLLOWING)
        {
            SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState.FOLLOWING, targetPosition);
        }
    }

    private void SetupStoppingDistanceState(FromSpawnerYaggiStandardStoppingDistanceState state, Vector3 destination)
    {
        switch (state)
        {
            case FromSpawnerYaggiStandardStoppingDistanceState.PUSHED:
                navMeshAgent.isStopped = false;
                navMeshAgent.stoppingDistance = 0;
                animator.SetFloat("Speed", -1f);
                break;
            case FromSpawnerYaggiStandardStoppingDistanceState.STAYING:
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case FromSpawnerYaggiStandardStoppingDistanceState.FOLLOWING:
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
        if (currentState is FromSpawnerYaggiStandardState.DYING)
        {
            return;
        }

        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.RemoveEnemyHealthUI(uiHealthBarAnchor);
            SwitchState(FromSpawnerYaggiStandardState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(uiHealthBarAnchor, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(FromSpawnerYaggiStandardState state)
    {
        if (state == FromSpawnerYaggiStandardState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is FromSpawnerYaggiStandardState.CHECKING or FromSpawnerYaggiStandardState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not FromSpawnerYaggiStandardState.DYING)
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
        Debug.Log("Is Dying");
        Destroy(transform.parent.gameObject);
    }

    public void HitByPlayerFrom(Vector3 hitDirection)
    {
        if (currentState is FromSpawnerYaggiStandardState.DYING or FromSpawnerYaggiStandardState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(FromSpawnerYaggiStandardState.CHECKING);
    }
}
