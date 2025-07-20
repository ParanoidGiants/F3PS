using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum YaggiShieldSpitterStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

public enum YaggiShieldSpitterState
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

public enum YaggiShieldSpitterAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    RECOVERY
}

public class YaggiShieldSpitterController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;

    public bool debugIsStopped;
    public float debugStoppingDistance;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public PatrolManager patrolManager;
    public TimeObject timeObject;
    public AnimateMesh animateMesh;
    public SensorController sensorController;
    public GameObject hittableParent;
    public Collider[] collidersThatShouldntBeHit;
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
    public YaggiShieldSpitterStoppingDistanceState stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.STAYING;
    public Vector3 lastTargetPosition = Vector3.zero;
    public float aggressiveRotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;

    [Header("Attack")]
    public Transform attackProjectileSpawnPoint;
    public ObjectPool attackprojectilePool;
    public int attackProjectileCount = 8;
    public float attackSpreadAngle = 20f;
    public float attackProjectileDistance = 5f;
    public float attackSpeed = 10f;
    public float attackGravityScale = 1f;

    public Vector3 _attackForward;
    public YaggiShieldSpitterAttackState attackState = YaggiShieldSpitterAttackState.NONE;
    public float coolDownTime;
    public float coolDownDuration;

    [Header("Attack Anticipation")]
    public AnimationClip attackAnticipationAnimation;
    public float attackAnticipationTime = 0f;
    public float attackAnticipationRotationSpeed = 100f;

    [Header("Attack Recovery")]
    public AnimationClip attackRecoveryAnimation;
    public float attackRecoveryTime = 0f;

    [Space(20)]
    [Header("Watchers")]
    public EnemyHealthUIPool _healthUIPool;
    public YaggiShieldSpitterState currentState = YaggiShieldSpitterState.IDLE;
    public bool isDead = false;

    protected void Awake()
    {
        var parent = transform.parent;
        attackprojectilePool.Init(parent);
        var projectiles = attackprojectilePool.GetObjects();
        foreach (var projectile in projectiles)
        {
            var projectileComponent = projectile.GetComponent<YaggiSpitProjectile>();
            projectileComponent.Init(parent.gameObject, collidersThatShouldntBeHit);
            projectile.SetActive(false);
        }
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();

        EnterState(YaggiShieldSpitterState.IDLE);
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
        _healthUIPool.CreateEnemyHealthUI(uiHealthBarAnchor);
    }

    private void SwitchState(YaggiShieldSpitterState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(YaggiShieldSpitterState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case YaggiShieldSpitterState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0);
                break;
            case YaggiShieldSpitterState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = walkSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 0.5f);
                break;
            case YaggiShieldSpitterState.AGGRESSIVE:
                stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = true;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = runSpeed * TimeScale;
                break;
            case YaggiShieldSpitterState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = runSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case YaggiShieldSpitterState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 0f);
                break;
            case YaggiShieldSpitterState.RETURN_TO_IDLE:
                break;
            case YaggiShieldSpitterState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case YaggiShieldSpitterState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(YaggiShieldSpitterState currentState)
    {
        switch (currentState)
        {
            case YaggiShieldSpitterState.IDLE:
                break;
            case YaggiShieldSpitterState.PATROLLING:
                break;
            case YaggiShieldSpitterState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 1000;
                break;
            case YaggiShieldSpitterState.CHECKING:
                break;
            case YaggiShieldSpitterState.SUSPICIOUS:
                break;
            case YaggiShieldSpitterState.RETURN_TO_IDLE:
                break;
            case YaggiShieldSpitterState.HIT:
                break;
            case YaggiShieldSpitterState.DYING:
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
            case YaggiShieldSpitterStoppingDistanceState.PUSHED:
                color = Color.red;
                break;
            case YaggiShieldSpitterStoppingDistanceState.FOLLOWING:
                color = Color.yellow;
                break;
            case YaggiShieldSpitterStoppingDistanceState.STAYING:
                color = Color.green;
                break;
        }
        Debug.DrawLine(transform.position, navMeshAgent.destination, color, 4f);
        debugIsStopped = navMeshAgent.isStopped;
        debugStoppingDistance = navMeshAgent.stoppingDistance;


        if (currentState != YaggiShieldSpitterState.DYING && currentState != YaggiShieldSpitterState.AGGRESSIVE && sensorController.HasTarget())
        {
            SwitchState(YaggiShieldSpitterState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case YaggiShieldSpitterState.IDLE:
                if (idleDuration < 0f || patrolManager.PatrolPointCount <= 0)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(YaggiShieldSpitterState.PATROLLING);
                break;

            case YaggiShieldSpitterState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(YaggiShieldSpitterState.IDLE);
                break;

            case YaggiShieldSpitterState.AGGRESSIVE:
                if (attackState != YaggiShieldSpitterAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                if (!sensorController.HasTarget())
                {
                    SwitchState(YaggiShieldSpitterState.CHECKING);
                    return;
                }
                var selectedTarget = sensorController.GetTargetFromSensors();
                lastTargetPosition = selectedTarget.Center();
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
                    attackState = YaggiShieldSpitterAttackState.INIT;
                }

                break;
            case YaggiShieldSpitterState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(YaggiShieldSpitterState.SUSPICIOUS);
                }
                break;
            case YaggiShieldSpitterState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(YaggiShieldSpitterState.IDLE);
                break;
            case YaggiShieldSpitterState.RETURN_TO_IDLE:
                break;
            case YaggiShieldSpitterState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(YaggiShieldSpitterState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case YaggiShieldSpitterState.DYING:
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
            case YaggiShieldSpitterAttackState.INIT:
                attackState = YaggiShieldSpitterAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                attackAnticipationTime = 0f;
                attackRecoveryTime = 0f;

                animator.SetTrigger("Charge");
                break;
            case YaggiShieldSpitterAttackState.ANTICIPATION:
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

                if (attackAnticipationTime >= attackAnticipationAnimation.length)
                {
                    attackState = YaggiShieldSpitterAttackState.RECOVERY;
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
            case YaggiShieldSpitterAttackState.RECOVERY:
                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryAnimation.length)
                {
                    attackState = YaggiShieldSpitterAttackState.NONE;
                    coolDownTime = 0f;
                    stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.STAYING;
                    navMeshAgent.isStopped = true;
                }
                break;
            case YaggiShieldSpitterAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }


    private void HandleAggressiveStoppingDistance(Vector3 targetPosition)
    {
        var toTarget = targetPosition - transform.position;
        var lookDirection = Vector3.ProjectOnPlane(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(lookDirection),
            aggressiveRotationSpeed * ScaledDeltaTime
        );


        if (!sensorController.IsTargetInLineOfSight() || !sensorController.HasTarget())
        {
            navMeshAgent.stoppingDistance = 0;
            return;
        }

        var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, targetPosition);
        if (stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            animator.SetFloat("Speed", 1f);
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.isStopped = false;
            stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.FOLLOWING;
        }
        else if (stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.destination = fleeDestination;
            navMeshAgent.isStopped = false;

            stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.PUSHED;
            animator.SetFloat("Speed", -1f);
        }
        else if (stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            stoppingDistanceState = YaggiShieldSpitterStoppingDistanceState.STAYING;
            navMeshAgent.isStopped = true;
            navMeshAgent.destination = targetPosition;
            animator.SetFloat("Speed", 0f);
        }
        else if (stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(targetPosition, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = targetPosition + fleeDirection * distance;
            navMeshAgent.destination = fleeDestination;
        }
        else if (stoppingDistanceState == YaggiShieldSpitterStoppingDistanceState.FOLLOWING)
        {
            navMeshAgent.destination = targetPosition;
        }
    }

    public virtual void Hit(int damage)
    {
        if (currentState is YaggiShieldSpitterState.DYING)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.RemoveEnemyHealthUI(uiHealthBarAnchor);
            SwitchState(YaggiShieldSpitterState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(uiHealthBarAnchor, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(YaggiShieldSpitterState state)
    {
        if (state == YaggiShieldSpitterState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is YaggiShieldSpitterState.CHECKING or YaggiShieldSpitterState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not YaggiShieldSpitterState.DYING)
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
        if (currentState is YaggiShieldSpitterState.DYING or YaggiShieldSpitterState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(YaggiShieldSpitterState.CHECKING);
    }
}
