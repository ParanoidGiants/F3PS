using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum YaggiSpitterStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

public enum YaggiSpitterState
{
    IDLE,
    AGGRESSIVE,
    CHECKING,
    SUSPICIOUS,
    RETURN_TO_IDLE,
    PATROLLING,
    HIT,
    DYING,
    DEAD
}

public enum YaggiSpitterAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    RECOVERY
}

public class YaggiSpitterController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;
    public bool IsDead => currentState is YaggiSpitterState.DYING or YaggiSpitterState.DEAD;

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
    public Transform uiHealthBarAnchor;
    public OnTouchHeal healthCollectable;

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
    public YaggiSpitterStoppingDistanceState stoppingDistanceState = YaggiSpitterStoppingDistanceState.STAYING;
    public float aggressiveMoveSpeed = 0f;
    public float aggressiveRotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;

    [Header("Attack")]
    public Transform attackProjectileSpawnPoint;
    public Quaternion attackProjectileSpawnPointInitialRotation;
    public ObjectPool attackprojectilePool;
    public int attackProjectileCount = 8;
    public float attackSpreadAngle = 20f;
    public float attackProjectileDistance = 5f;
    public float attackSpeed = 10f;
    public float attackGravityScale = 1f;

    public Vector3 _attackForward;
    public YaggiSpitterAttackState attackState = YaggiSpitterAttackState.NONE;
    public float coolDownTime;
    public float coolDownDuration;

    [Header("Attack Anticipation")]
    public AnimationClip attackAnticipationAnimationClip;
    public float attackAnticipationTime = 0f;
    public float attackAnticipationRotationSpeed = 100f;

    [Header("Attack Recovery")]
    public AnimationClip attackRecoveryAnimationClip;
    public float attackRecoveryTime = 0f;

    [Space(20)]
    [Header("Watchers")]
    public EnemyHealthUIPool _healthUIPool;
    public YaggiSpitterState currentState = YaggiSpitterState.IDLE;
    private Vector3 lastTargetPosition;

    protected void Awake()
    {
        attackProjectileSpawnPointInitialRotation = attackProjectileSpawnPoint.rotation;
        navMeshAgent.Warp(transform.position);
        navMeshAgent.SetDestination(transform.position);

        var parent = transform.parent;
        attackprojectilePool.Init(parent);
        var projectiles = attackprojectilePool.GetObjects();
        foreach (var projectile in projectiles)
        {
            var projectileComponent = projectile.GetComponent<YaggiSpitProjectile>();
            projectileComponent.Init(parent.gameObject, hittableParent.GetComponents<Collider>());
            projectile.SetActive(false);
        }
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();
        EnterState(YaggiSpitterState.IDLE);
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
        _healthUIPool.CreateEnemyHealthUI(uiHealthBarAnchor);
    }

    private void SwitchState(YaggiSpitterState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(YaggiSpitterState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case YaggiSpitterState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0);
                break;
            case YaggiSpitterState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiSpitterState.AGGRESSIVE:
                stoppingDistanceState = YaggiSpitterStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = true;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = aggressiveMoveSpeed * TimeScale;
                break;
            case YaggiSpitterState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiSpitterState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 0f);
                break;
            case YaggiSpitterState.RETURN_TO_IDLE:
                break;
            case YaggiSpitterState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case YaggiSpitterState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                healthCollectable.gameObject.SetActive(true);
                break;
            case YaggiSpitterState.DEAD:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(YaggiSpitterState currentState)
    {
        if (currentState is YaggiSpitterState.AGGRESSIVE)
        {
            navMeshAgent.angularSpeed = 1000;
        }
    }


    private void FixedUpdate()
    {
        if (currentState is YaggiSpitterState.DEAD)
        {
            return;
        }

        debugIsStopped = navMeshAgent.isStopped;
        debugStoppingDistance = navMeshAgent.stoppingDistance;


        if (currentState != YaggiSpitterState.DYING && currentState != YaggiSpitterState.AGGRESSIVE && sensorController.HasTarget())
        {
            SwitchState(YaggiSpitterState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case YaggiSpitterState.IDLE:
                if (idleDuration < 0f || patrolManager.PatrolPointCount <= 0)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(YaggiSpitterState.PATROLLING);
                break;

            case YaggiSpitterState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(YaggiSpitterState.IDLE);
                break;

            case YaggiSpitterState.AGGRESSIVE:
                if (attackState != YaggiSpitterAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                if (!sensorController.HasTarget())
                {
                    SwitchState(YaggiSpitterState.CHECKING);
                    return;
                }
                lastTargetPosition = sensorController.GetTargetFromSensors().Center();
                checkingDestination = lastTargetPosition;
                HandleAggressiveStoppingDistance(lastTargetPosition);

                var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, lastTargetPosition);
                var canAttack = coolDownTime >= coolDownDuration && distanceToTarget <= attackProjectileDistance;
                if (!canAttack)
                {
                    coolDownTime += ScaledDeltaTime;
                    break;
                }

                var targetDirection = (lastTargetPosition - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);
                if (isAlignedWithTarget)
                {
                    attackState = YaggiSpitterAttackState.INIT;
                }

                break;
            case YaggiSpitterState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(YaggiSpitterState.SUSPICIOUS);
                }
                break;
            case YaggiSpitterState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(YaggiSpitterState.IDLE);
                break;
            case YaggiSpitterState.RETURN_TO_IDLE:
                break;
            case YaggiSpitterState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(YaggiSpitterState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case YaggiSpitterState.DYING:
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
                SwitchState(YaggiSpitterState.DEAD);
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
            case YaggiSpitterAttackState.INIT:
                attackState = YaggiSpitterAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                attackAnticipationTime = 0f;
                attackRecoveryTime = 0f;

                animator.SetTrigger("Charge");
                break;
            case YaggiSpitterAttackState.ANTICIPATION:
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

                var targetDirectionForPitch = targetPosition - attackProjectileSpawnPoint.position;
                var horizontalDistance = new Vector3(targetDirectionForPitch.x, 0, targetDirectionForPitch.z).magnitude;
                var verticalDistance = targetDirectionForPitch.y;
                var desiredPitchAngle = -Mathf.Atan2(verticalDistance, horizontalDistance) * Mathf.Rad2Deg;
                var currentSpawnPointEuler = attackProjectileSpawnPoint.localEulerAngles;
                var clampedDesiredPitch = Mathf.Clamp(desiredPitchAngle, -80f, 80f);
                var targetProjectileSpawnPointRotation = Quaternion.Euler(clampedDesiredPitch, currentSpawnPointEuler.y, currentSpawnPointEuler.z);
                attackProjectileSpawnPoint.localRotation = Quaternion.RotateTowards(
                    attackProjectileSpawnPoint.localRotation,
                    targetProjectileSpawnPointRotation,
                    ScaledDeltaTime * attackAnticipationRotationSpeed
                );

                attackAnticipationTime += ScaledDeltaTime;

                if (attackAnticipationTime >= attackAnticipationAnimationClip.length)
                {
                    attackState = YaggiSpitterAttackState.RECOVERY;
                    animator.SetTrigger("Recover");
                    var yRotation = -attackSpreadAngle;
                    var yRotationStep = (2f * attackSpreadAngle) / attackProjectileCount;
                    for (int i = 0; i < attackProjectileCount; i++)
                    {
                        Quaternion projectileOrientation = Quaternion.Euler(0f, yRotation, 0f) * attackProjectileSpawnPoint.rotation;
                        var projectileObject = attackprojectilePool.GetObject();
                        var projectileTransform = projectileObject.transform;
                        projectileTransform.position = attackProjectileSpawnPoint.position;
                        projectileTransform.rotation = projectileOrientation;
                        var projectileComponent = projectileObject.GetComponent<YaggiSpitProjectile>();
                        projectileObject.SetActive(true);
                        projectileComponent.Shoot(attackSpeed, attackGravityScale);
                        yRotation += yRotationStep;
                    }
                }
                break;
            case YaggiSpitterAttackState.RECOVERY:
                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryAnimationClip.length)
                {
                    attackState = YaggiSpitterAttackState.NONE;
                    coolDownTime = 0f;
                    stoppingDistanceState = YaggiSpitterStoppingDistanceState.STAYING;
                    navMeshAgent.isStopped = true;
                }
                break;
            case YaggiSpitterAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
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
        if (stoppingDistanceState == YaggiSpitterStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState.FOLLOWING, targetPosition);
        }
        else if (stoppingDistanceState == YaggiSpitterStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaggiSpitterStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == YaggiSpitterStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState.STAYING, targetPosition);
        }
        else if (stoppingDistanceState == YaggiSpitterStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaggiSpitterStoppingDistanceState.FOLLOWING)
        {
            SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState.FOLLOWING, targetPosition);
        }
    }

    private void SetupStoppingDistanceState(YaggiSpitterStoppingDistanceState state, Vector3 destination)
    {
        switch (state)
        {
            case YaggiSpitterStoppingDistanceState.PUSHED:
                navMeshAgent.isStopped = false;
                navMeshAgent.stoppingDistance = 0;
                animator.SetFloat("Speed", -1f);
                break;
            case YaggiSpitterStoppingDistanceState.STAYING:
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case YaggiSpitterStoppingDistanceState.FOLLOWING:
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
        if (navMeshDestination != null)
        {
            navMeshDestination.position = position;
        }
        navMeshAgent.destination = position;
    }

    public virtual void Hit(int damage)
    {
        if (currentState is YaggiSpitterState.DYING or YaggiSpitterState.DEAD)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.RemoveEnemyHealthUI(uiHealthBarAnchor);
            SwitchState(YaggiSpitterState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(uiHealthBarAnchor, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(YaggiSpitterState state)
    {
        if (state == YaggiSpitterState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is YaggiSpitterState.CHECKING or YaggiSpitterState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not YaggiSpitterState.DYING)
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
        if (currentState is YaggiSpitterState.DYING or YaggiSpitterState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(YaggiSpitterState.CHECKING);
    }
}
