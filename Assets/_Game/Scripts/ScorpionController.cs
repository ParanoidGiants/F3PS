using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum ScorpionStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

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
    public PatrolManager patrolManager;
    public TimeObject timeObject;
    public AnimateMesh animateMesh;
    public SensorController sensorController;
    public GameObject hittableParent;
    public GameObject chargeFlare;
    public Collider attackHitBox;
    public Transform uiHealthBarAnchor;

    [Space(20)]
    [Header("Settings")]
    public int health;
    public int maxHealth = 100;
    public float walkSpeed;
    public float runSpeed;

    [Space(10)]
    [Header("Idle Settings")]
    public float idleDuration = 0f;
    public float idleTime = 0f;

    [Space(10)]
    [Header("Checking Settings")]
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
    public ScorpionStoppingDistanceState stoppingDistanceState = ScorpionStoppingDistanceState.STAYING;
    public float rotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;

    [Header("Attack")]
    public ScorpionAttackState attackState = ScorpionAttackState.NONE;
    public float coolDownTime;
    public float coolDownDuration;
    public bool hitTargetEarly = false;
    public Vector3 _attackForward;
    public Vector3 lastTargetPosition;

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
    public bool isDead = false;

    protected void Awake()
    {
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();
        EnterState(ScorpionState.IDLE);
        
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
        _healthUIPool.CreateEnemyHealthUI(uiHealthBarAnchor);
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
                navMeshAgent.speed = walkSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case ScorpionState.AGGRESSIVE:
                stoppingDistanceState = ScorpionStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = false;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = runSpeed * TimeScale;
                break;
            case ScorpionState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = runSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case ScorpionState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 1f);
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case ScorpionState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(ScorpionState currentState)
    {
        if (currentState is ScorpionState.AGGRESSIVE)
        {
            navMeshAgent.angularSpeed = 1000;
        }
    }

    private void FixedUpdate()
    {
        if (currentState != ScorpionState.DYING && currentState != ScorpionState.AGGRESSIVE && sensorController.HasTarget())
        {
            SwitchState(ScorpionState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case ScorpionState.IDLE:
                if (idleDuration < 0f || patrolManager.PatrolPointCount <= 0)
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

                if (!sensorController.HasTarget())
                {
                    SwitchState(ScorpionState.CHECKING);
                    return;
                }

                lastTargetPosition = sensorController.GetTargetFromSensors().Center();
                HandleAggressiveStoppingDistance(lastTargetPosition);

                var canAttack = coolDownTime >= coolDownDuration && Helper.HasReachedDestination(navMeshAgent);
                if (!canAttack)
                {
                    coolDownTime += ScaledDeltaTime;
                    break;
                }

                var targetDirection = (lastTargetPosition - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);

                if (isAlignedWithTarget)
                {
                    attackState = ScorpionAttackState.INIT;
                }

                break;
            case ScorpionState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(ScorpionState.SUSPICIOUS);
                }
                break;
            case ScorpionState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(ScorpionState.IDLE);
                break;
            case ScorpionState.RETURN_TO_IDLE:
                break;
            case ScorpionState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(ScorpionState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case ScorpionState.DYING:
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
                var targetPosition = lastTargetPosition;
                if (sensorController.HasTarget())
                {
                    targetPosition = sensorController.GetTargetFromSensors().Center();
                }
                var lookDirection = targetPosition - transform.position;
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
                    attackState = ScorpionAttackState.EXECUTION;
                    chargeFlare.SetActive(false);
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
                    SetupStoppingDistanceState(stoppingDistanceState, navMeshAgent.destination);
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

    private void HandleAggressiveStoppingDistance(Vector3 targetPosition)
    {
        var selectedTarget = sensorController.GetTargetFromSensors();
        var toTarget = targetPosition - transform.position;
        var lookDirection = Vector3.ProjectOnPlane(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(lookDirection),
            rotationSpeed * ScaledDeltaTime
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
        if (stoppingDistanceState == ScorpionStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            SetupStoppingDistanceState(ScorpionStoppingDistanceState.FOLLOWING, targetPosition);
        }
        else if (stoppingDistanceState == ScorpionStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(ScorpionStoppingDistanceState.PUSHED, fleeDestination);
            animator.SetFloat("Speed", -1f);
        }
        else if (stoppingDistanceState == ScorpionStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == ScorpionStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            SetupStoppingDistanceState(ScorpionStoppingDistanceState.STAYING, targetPosition);
            animator.SetFloat("Speed", 0f);
        }
        else if (stoppingDistanceState == ScorpionStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(ScorpionStoppingDistanceState.PUSHED, fleeDestination);
            animator.SetFloat("Speed", -1f);
        }
        else if (stoppingDistanceState == ScorpionStoppingDistanceState.FOLLOWING)
        {
            SetupStoppingDistanceState(ScorpionStoppingDistanceState.FOLLOWING, targetPosition);
            animator.SetFloat("Speed", 1f);
        }
    }

    private void SetupStoppingDistanceState(ScorpionStoppingDistanceState state, Vector3 destination)
    {
        switch (state)
        {
            case ScorpionStoppingDistanceState.PUSHED:
                navMeshAgent.isStopped = false;
                navMeshAgent.stoppingDistance = 0;
                animator.SetFloat("Speed", -1f);
                break;
            case ScorpionStoppingDistanceState.STAYING:
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case ScorpionStoppingDistanceState.FOLLOWING:
                navMeshAgent.stoppingDistance = 0;
                navMeshAgent.isStopped = false;
                animator.SetFloat("Speed", 1f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        stoppingDistanceState = state;
        navMeshAgent.destination = destination;
    }

    public virtual void Hit(int damage)
    {
        if (currentState is ScorpionState.DYING)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.RemoveEnemyHealthUI(uiHealthBarAnchor);
            SwitchState(ScorpionState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(uiHealthBarAnchor, health, maxHealth);
        animateMesh.HitFlash();
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
        else if (state is not ScorpionState.DYING)
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
        Destroy(transform.parent.gameObject);
    }

    public void HitByPlayerFrom(Vector3 hitDirection)
    {
        if (currentState is ScorpionState.DYING or ScorpionState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(ScorpionState.CHECKING);
    }
}
