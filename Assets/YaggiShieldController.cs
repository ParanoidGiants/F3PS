using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum YaggiShieldState
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

public enum YaggiShieldAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    EXECUTION,
    RECOVERY
}

public class YaggiShieldController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public PatrolManager patrolManager;
    public TimeObject timeObject;
    public AnimateMesh animateMesh;
    public SensorController sensorController;
    public GameObject hittableParent;
    public GameObject chargeFlare;
    public Collider attackHitBox;

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
    public Hittable _selectedTarget;
    public float rotationSpeed;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;
    [Header("Attack")]
    public YaggiShieldAttackState attackState = YaggiShieldAttackState.NONE;
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
    public YaggiShieldState currentState = YaggiShieldState.IDLE;
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
        EnterState(YaggiShieldState.IDLE);
    }

    private void SwitchState(YaggiShieldState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(YaggiShieldState state)
    {
        var traveledDistance = Vector3.Distance(lastPosition, transform.position);
        animator.SetFloat("Speed", traveledDistance > 0.1f ? 1 : 0);
        lastPosition = transform.position;

        UpdateSensorState(state);
        switch (state)
        {
            case YaggiShieldState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                break;
            case YaggiShieldState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                break;
            case YaggiShieldState.AGGRESSIVE:
                navMeshAgent.isStopped = false;
                navMeshAgent.angularSpeed = 0f;
                break;
            case YaggiShieldState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiShieldState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiShieldState.RETURN_TO_IDLE:
                break;
            case YaggiShieldState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case YaggiShieldState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(YaggiShieldState currentState)
    {
        switch (currentState)
        {
            case YaggiShieldState.IDLE:
                break;
            case YaggiShieldState.PATROLLING:
                break;
            case YaggiShieldState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 1000;
                break;
            case YaggiShieldState.CHECKING:
                break;
            case YaggiShieldState.SUSPICIOUS:
                break;
            case YaggiShieldState.RETURN_TO_IDLE:
                break;
            case YaggiShieldState.HIT:
                break;
            case YaggiShieldState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void FixedUpdate()
    {
        if (currentState != YaggiShieldState.DYING && currentState != YaggiShieldState.AGGRESSIVE && sensorController.IsTargetDetected())
        {
            SwitchState(YaggiShieldState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case YaggiShieldState.IDLE:
                if (idleDuration < 0f)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(YaggiShieldState.PATROLLING);
                break;

            case YaggiShieldState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(YaggiShieldState.IDLE);
                break;

            case YaggiShieldState.AGGRESSIVE:
                if (attackState != YaggiShieldAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                bool hasTarget = sensorController.IsTargetDetected();
                if (!hasTarget)
                {
                    SwitchState(YaggiShieldState.CHECKING);
                    return;
                }

                _selectedTarget = sensorController.GetTargetFromSensors();
                HandleAggressiveStoppingDistance();

                var canAttack = coolDownTime >= coolDownDuration && Helper.HasReachedDestination(navMeshAgent);
                if (!canAttack)
                {
                    coolDownTime += ScaledDeltaTime;
                    break;
                }

                var targetDirection = (_selectedTarget.Center() - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);

                if (isAlignedWithTarget)
                {
                    attackState = YaggiShieldAttackState.INIT;
                }

                break;
            case YaggiShieldState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(YaggiShieldState.SUSPICIOUS);
                }
                break;
            case YaggiShieldState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(YaggiShieldState.IDLE);
                break;
            case YaggiShieldState.RETURN_TO_IDLE:
                break;
            case YaggiShieldState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(YaggiShieldState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case YaggiShieldState.DYING:
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
            case YaggiShieldAttackState.INIT:
                attackState = YaggiShieldAttackState.ANTICIPATION;

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
            case YaggiShieldAttackState.ANTICIPATION:
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
                    _attackForward = transform.forward;
                    attackState = YaggiShieldAttackState.EXECUTION;
                    chargeFlare.SetActive(false);
                    attackHitBox.enabled = true;
                    attackExecutionStartPosition = transform.position;
                    attackExecutionEndPosition = attackExecutionStartPosition + _attackForward * attackExecutionDistance;
                    MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", transform);
                    animator.SetTrigger("Attack");
                }
                break;
            case YaggiShieldAttackState.EXECUTION:
                transform.position = Vector3.Lerp(
                    attackExecutionStartPosition,
                    attackExecutionEndPosition,
                    attackExecutionTime / attackExecutionDuration
                );

                attackExecutionTime += ScaledDeltaTime;
                if (hitTargetEarly || attackExecutionTime >= attackExecutionDuration)
                {
                    attackState = YaggiShieldAttackState.RECOVERY;
                    attackHitBox.enabled = false;
                    attackRecoveryStartPosition = transform.position;
                    attackRecoveryEndPosition = attackRecoveryStartPosition - _attackForward * attackRecoveryDistance;
                    animator.SetTrigger("Recover");
                }
                break;
            case YaggiShieldAttackState.RECOVERY:
                transform.position = Vector3.Lerp(
                    attackRecoveryStartPosition,
                    attackRecoveryEndPosition,
                    attackRecoveryTime / attackRecoveryDuration
                );

                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryDuration)
                {
                    attackState = YaggiShieldAttackState.NONE;
                    coolDownTime = 0f;
                    navMeshAgent.isStopped = false;
                }
                break;
            case YaggiShieldAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case YaggiShieldState.IDLE:
                break;
            case YaggiShieldState.AGGRESSIVE:
                break;
            case YaggiShieldState.CHECKING:
                break;
            case YaggiShieldState.SUSPICIOUS:
                break;
            case YaggiShieldState.RETURN_TO_IDLE:
                break;
            case YaggiShieldState.PATROLLING:
                break;
            case YaggiShieldState.HIT:
                break;
            case YaggiShieldState.DYING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void HandleAggressiveStoppingDistance()
    {
        var toTarget = _selectedTarget.Center() - transform.position;
        var distanceToTarget = toTarget.magnitude;
        var lookDirection = Vector3.ProjectOnPlane(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * ScaledDeltaTime);
        if (!sensorController.IsTargetInLineOfSight())
        {
            navMeshAgent.stoppingDistance = 0;
            return;
        }

        else if (_selectedTarget == null)
        {
            Debug.LogWarning("Selected target is null, cannot handle aggressive stopping distance.");
            return;
        }
        else
        {
            if (distanceToTarget <= stoppingDistanceStay)
            {
                var fleeDirection = -lookDirection;
                var ray = new Ray(transform.position, fleeDirection);
                Physics.Raycast(ray, out var hit, stoppingDistanceFollow);
                var distance = Mathf.Max(stoppingDistanceFollow, hit.distance);
                Debug.Log(distance);
                Debug.DrawLine(transform.position, transform.position + fleeDirection * distance, Color.red, 1f);
                var fleeDestination = transform.position + fleeDirection * distance;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = fleeDestination;
            }
            else
            {
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.destination = _selectedTarget.Center();
            }
        }
    }

    public virtual void Hit(int damage)
    {
        Debug.Log("YaggiShield Hit: " + damage);
        if (currentState is YaggiShieldState.DYING)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.OnKillTarget(transform);
            SwitchState(YaggiShieldState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(transform, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(YaggiShieldState state)
    {
        if (state == YaggiShieldState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is YaggiShieldState.CHECKING or YaggiShieldState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not YaggiShieldState.DYING)
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
        if (currentState is YaggiShieldState.DYING or YaggiShieldState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(YaggiShieldState.CHECKING);
    }
}
