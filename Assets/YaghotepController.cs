using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum YaghotepStoppingDistanceState
{
    PUSHED,
    FOLLOWING,
    STAYING
}

public enum YaghotepState
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

public enum YaghotepAttackState
{
    NONE,
    INIT,
    ANTICIPATION,
    RECOVERY
}

public class YaghotepController : MonoBehaviour
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
    public YaghotepStoppingDistanceState stoppingDistanceState = YaghotepStoppingDistanceState.STAYING;
    public Hittable _selectedTarget;
    public float aggressiveMoveSpeed = 0f;
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
    public YaghotepAttackState attackState = YaghotepAttackState.NONE;
    public float coolDownTime;
    public float coolDownDuration;

    [Header("Attack Anticipation")]
    public float attackAnticipationTime = 0f;
    public float attackAnticipationDuration = 1f;
    public float attackAnticipationRotationSpeed = 100f;

    [Header("Attack Recovery")]
    public float attackRecoveryTime = 0f;
    public float attackRecoveryDuration = 1f;

    [Space(20)]
    [Header("Watchers")]
    public EnemyHealthUIPool _healthUIPool;
    public YaghotepState currentState = YaghotepState.IDLE;
    public bool isDead = false;
    private Vector3 lastPosition;

    protected void Awake()
    {
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();

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
        EnterState(YaghotepState.IDLE);
    }

    private void SwitchState(YaghotepState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(currentState);
    }

    private void EnterState(YaghotepState state)
    {
        UpdateSensorState(state);
        switch (state)
        {
            case YaghotepState.IDLE:
                idleTime = 0f;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0);
                break;
            case YaghotepState.PATROLLING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = patrolMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                patrolManager.SetNextPatrolPoint();
                navMeshAgent.destination = patrolManager.CurrentPatrolPoint;
                animator.SetFloat("Speed", 1f);
                break;
            case YaghotepState.AGGRESSIVE:
                stoppingDistanceState = YaghotepStoppingDistanceState.STAYING;
                navMeshAgent.isStopped = true;
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = aggressiveMoveSpeed * TimeScale;
                break;
            case YaghotepState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                navMeshAgent.destination = checkingDestination;
                animator.SetFloat("Speed", 1f);
                break;
            case YaghotepState.SUSPICIOUS:
                suspiciousTime = suspiciousDuration;
                _startRotation = transform.rotation;
                animator.SetFloat("Speed", 0f);
                break;
            case YaghotepState.RETURN_TO_IDLE:
                break;
            case YaghotepState.HIT:
                hitTime = stunDuration;
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Hit");
                break;
            case YaghotepState.DYING:
                navMeshAgent.isStopped = true;
                animator.SetTrigger("Die");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ExitState(YaghotepState currentState)
    {
        switch (currentState)
        {
            case YaghotepState.IDLE:
                break;
            case YaghotepState.PATROLLING:
                break;
            case YaghotepState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 1000;
                break;
            case YaghotepState.CHECKING:
                break;
            case YaghotepState.SUSPICIOUS:
                break;
            case YaghotepState.RETURN_TO_IDLE:
                break;
            case YaghotepState.HIT:
                break;
            case YaghotepState.DYING:
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
            case YaghotepStoppingDistanceState.PUSHED:
                color = Color.red;
                break;
            case YaghotepStoppingDistanceState.FOLLOWING:
                color = Color.yellow;
                break;
            case YaghotepStoppingDistanceState.STAYING:
                color = Color.green;
                break;
        }
        Debug.DrawLine(transform.position, navMeshAgent.destination, color, 4f);
        debugIsStopped = navMeshAgent.isStopped;
        debugStoppingDistance = navMeshAgent.stoppingDistance;


        if (currentState != YaghotepState.DYING && currentState != YaghotepState.AGGRESSIVE && sensorController.IsTargetDetected())
        {
            SwitchState(YaghotepState.AGGRESSIVE);
        }


        switch (currentState)
        {
            case YaghotepState.IDLE:
                if (idleDuration < 0f)
                {
                    return;
                }
                idleTime += ScaledDeltaTime;
                if (idleDuration > idleTime)
                {
                    return;
                }
                SwitchState(YaghotepState.PATROLLING);
                break;

            case YaghotepState.PATROLLING:
                if (!Helper.HasReachedDestination(navMeshAgent)) return;
                SwitchState(YaghotepState.IDLE);
                break;

            case YaghotepState.AGGRESSIVE:
                if (attackState != YaghotepAttackState.NONE)
                {
                    HandleAttackProcedure();
                    return;
                }

                bool hasTarget = sensorController.IsTargetDetected();
                if (!hasTarget)
                {
                    SwitchState(YaghotepState.CHECKING);
                    return;
                }
                _selectedTarget = sensorController.GetTargetFromSensors();
                checkingDestination = _selectedTarget.Center();

                HandleAggressiveStoppingDistance();

                var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, _selectedTarget.Center());
                var canAttack = coolDownTime >= coolDownDuration && distanceToTarget <= attackProjectileDistance;
                if (!canAttack)
                {
                    coolDownTime += ScaledDeltaTime;
                    break;
                }

                var targetDirection = (_selectedTarget.Center() - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);
                if (isAlignedWithTarget)
                {
                    attackState = YaghotepAttackState.INIT;
                }

                break;
            case YaghotepState.CHECKING:
                if (Helper.HasReachedDestination(navMeshAgent))
                {
                    SwitchState(YaghotepState.SUSPICIOUS);
                }
                break;
            case YaghotepState.SUSPICIOUS:
                suspiciousTime -= ScaledDeltaTime;

                float isSuspiciousAnimateTime = Mathf.Sin(suspiciousTime / suspiciousDuration * (2f * Mathf.PI));
                transform.rotation = _startRotation * Quaternion.Euler(0, suspiciousRotateSpeed * isSuspiciousAnimateTime, 0f);

                if (suspiciousTime > 0f) return;

                SwitchState(YaghotepState.IDLE);
                break;
            case YaghotepState.RETURN_TO_IDLE:
                break;
            case YaghotepState.HIT:
                if (hitTime < 0f)
                {
                    SwitchState(YaghotepState.PATROLLING);
                    return;
                }
                hitTime -= ScaledDeltaTime;
                break;

            case YaghotepState.DYING:
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
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;

                navMeshAgent.isStopped = true;
                attackAnticipationTime = 0f;
                attackRecoveryTime = 0f;

                animator.SetTrigger("Charge");
                break;
            case YaghotepAttackState.ANTICIPATION:
                var targetPosition = _selectedTarget.Center();
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

                if (attackAnticipationTime >= attackAnticipationDuration)
                {
                    attackState = YaghotepAttackState.RECOVERY;
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
            case YaghotepAttackState.RECOVERY:
                attackRecoveryTime += ScaledDeltaTime;
                if (attackRecoveryTime >= attackRecoveryDuration)
                {
                    attackState = YaghotepAttackState.NONE;
                    coolDownTime = 0f;
                    stoppingDistanceState = YaghotepStoppingDistanceState.STAYING;
                    navMeshAgent.isStopped = true;
                }
                break;
            case YaghotepAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }


    private void HandleAggressiveStoppingDistance()
    {
        var toTarget = _selectedTarget.Center() - transform.position;
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

        if (_selectedTarget == null)
        {
            return;
        }

        var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, _selectedTarget.Center());
        if (stoppingDistanceState == YaghotepStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            animator.SetFloat("Speed", 1f);
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.isStopped = false;
            stoppingDistanceState = YaghotepStoppingDistanceState.FOLLOWING;
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            navMeshAgent.stoppingDistance = 0;
            navMeshAgent.destination = fleeDestination;
            navMeshAgent.isStopped = false;

            stoppingDistanceState = YaghotepStoppingDistanceState.PUSHED;
            animator.SetFloat("Speed", -1f);
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == YaghotepStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            stoppingDistanceState = YaghotepStoppingDistanceState.STAYING;
            navMeshAgent.isStopped = true;
            navMeshAgent.destination = _selectedTarget.Center();
            animator.SetFloat("Speed", 0f);
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            navMeshAgent.destination = fleeDestination;
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.FOLLOWING)
        {
            navMeshAgent.destination = _selectedTarget.Center();
        }
    }

    public virtual void Hit(int damage)
    {
        Debug.Log("Yaghotep Hit: " + damage);
        if (currentState is YaghotepState.DYING)
        {
            return;
        }
        health -= damage;
        MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
        if (health <= 0)
        {
            _healthUIPool.OnKillTarget(transform);
            SwitchState(YaghotepState.DYING);
            return;
        }
        _healthUIPool.OnHitTarget(transform, health, maxHealth);
        animateMesh.HitFlash();
    }

    private void UpdateSensorState(YaghotepState state)
    {
        if (state == YaghotepState.AGGRESSIVE)
        {
            sensorController.SetState(SensorState.AGGRESSIVE);
        }
        else if (state is YaghotepState.CHECKING or YaghotepState.SUSPICIOUS)
        {
            sensorController.SetState(SensorState.SEARCHING);
        }
        else if (state is not YaghotepState.DYING)
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
        if (currentState is YaghotepState.DYING or YaghotepState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = navMeshAgent.transform.position - hitDirection;
        SwitchState(YaghotepState.CHECKING);
    }
}
