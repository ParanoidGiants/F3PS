using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum YaghotepAttackType
{
    SPAWN,
    FORMATION
}

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
    EXECUTE,
    RECOVERY
}

public class YaghotepController : MonoBehaviour
{
    public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
    public float TimeScale => timeObject.currentTimeScale;

    [Header("Debug")]
    public Transform navMeshDestination;
    public EnemyHealthUIPool _healthUIPool;
    public YaghotepState currentState = YaghotepState.IDLE;
    public bool isDead = false;
    public Vector3 lastPosition;
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

    public YaghotepAttackType currentAttackType = YaghotepAttackType.SPAWN;
    public YaghotepAttackState attackState = YaghotepAttackState.NONE;
    public float attackProjectileDistance = 5f;
    public float spawnAttackCoolDownTime;
    public float spawnAttackCoolDownDuration;
    public Vector3 attackForward;

    #region Spawn Attack
    [Header("Spawn Attack")]
    public ObjectPool spawnAttackprojectilePool;
    public Transform spawnProjectileSpawnPoint;
    public int spawnAttackProjectileCount = 8;
    public float spawnAttackSpreadAngle = 20f;
    public float spawnAttackSpeed = 10f;
    public float spawnAttackGravityScale = 1f;
    public float spawnAttackAdditionalPitch = 30f;
    public int spawnMaximumEnemies = 10;

    [Header("Attack Anticipation")]
    public float spawnAttackAnticipationTime = 0f;
    public float spawnAttackAnticipationDuration = 1f;
    public float spawnAttackAnticipationRotationSpeed = 100f;

    [Header("Attack Recovery")]
    public float spawnAttackRecoveryTime = 0f;
    public float spawnAttackRecoveryDuration = 1f;

    public List<GameObject> spawnedEnemies;

    #endregion Spawn Attack

    #region Formation Attack
    [Header("Formation Attack")]
    public AnimationClip formationChargeAnimationClip;
    public AnimationClip formationExecutionAnimationClip;

    public ObjectPool formationAttackprojectilePool;
    public Transform formationProjectileSpawnPoint;
    public int formationAttackProjectileCount = 8;
    public float formationAttackSpreadAngle = 20f;
    public float formationAttackProjectileDistance = 5f;
    public float formationAttackSpeed = 10f;
    public float formationAttackGravityScale = 1f;
    public float additionalPitch = 30f;
    public float formationRadius = 3f;

    public Vector3 _formationAttackForward;
    public float formationAttackCoolDownTime;
    public float formationAttackCoolDownDuration = 8f;

    [Header("Formation Attack Anticipation")]
    public float formationAttackAnticipationTime = 0f;
    public float formationAttackAnticipationDuration = 1f;
    public float formationAttackAnticipationRotationSpeed = 100f;

    [Header("Formation Attack Execution")]
    public int formationAttackExecutionCount = 0;
    public int formationAttackExecutionTotalNumber = 8;
    public float formationAttackExecutionTime = 0f;
    public float formationAttackExecutionDuration = 1f;
    public float formationAttackExecutionRotationSpeed = 100f;

    [Header("Formation Attack Recovery")]
    public float formationAttackRecoveryTime = 0f;
    public float formationAttackRecoveryDuration = 1f;
    #endregion Formation Attack

    protected void Awake()
    {
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();

        var parent = transform.parent;
        spawnAttackprojectilePool.Init(parent);
        formationAttackprojectilePool.Init(parent);

        var spawnProjectiles = spawnAttackprojectilePool.GetObjects();
        foreach (var projectile in spawnProjectiles)
        {
            var projectileComponent = projectile.GetComponent<YaghotepSpawnProjectile>();
            projectileComponent.Init(parent.gameObject, collidersThatShouldntBeHit);
            projectile.SetActive(false);
        }

        var formationProjectiles = formationAttackprojectilePool.GetObjects();
        foreach (var projectile in formationProjectiles)
        {
            var projectileComponent = projectile.GetComponent<YaghotepProjectile>();
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
                SetDestination(patrolManager.CurrentPatrolPoint);
                animator.SetFloat("Speed", 1f);
                break;
            case YaghotepState.AGGRESSIVE:
                navMeshAgent.angularSpeed = 0f;
                navMeshAgent.speed = aggressiveMoveSpeed * TimeScale;
                break;
            case YaghotepState.CHECKING:
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = checkingMoveSpeed * TimeScale;
                navMeshAgent.stoppingDistance = 0f;
                SetDestination(checkingDestination);
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

                if (attackState is YaghotepAttackState.NONE && !sensorController.IsTargetDetected())
                {
                    SwitchState(YaghotepState.CHECKING);
                    return;
                }

                if (currentAttackType is YaghotepAttackType.SPAWN && attackState is not YaghotepAttackState.NONE)
                {
                    HandleSpawnAttackProcedure();
                    return;
                }
                else if (currentAttackType is YaghotepAttackType.FORMATION && attackState is not YaghotepAttackState.NONE)
                {
                    HandleFormationAttackProcedure();
                    return;
                }


                _selectedTarget = sensorController.GetTargetFromSensors();
                checkingDestination = _selectedTarget.Center();
                HandleAggressiveStoppingDistance();

                if (spawnedEnemies.Count == 0 && currentAttackType is not YaghotepAttackType.SPAWN)
                {
                    currentAttackType = YaghotepAttackType.SPAWN;
                }
                else if (spawnedEnemies.Count >= spawnMaximumEnemies && currentAttackType is YaghotepAttackType.SPAWN)
                {
                    currentAttackType = YaghotepAttackType.FORMATION;
                }

                if (currentAttackType is YaghotepAttackType.SPAWN)
                {
                    spawnAttackCoolDownTime += ScaledDeltaTime;
                }
                else
                {
                    formationAttackCoolDownTime += ScaledDeltaTime;
                }

                var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, _selectedTarget.Center());
                if (distanceToTarget >= attackProjectileDistance)
                {
                    return;
                }

                var targetDirection = (_selectedTarget.Center() - transform.position).normalized;
                bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);
                if (!isAlignedWithTarget)
                {
                    return;
                }

                if (currentAttackType is YaghotepAttackType.SPAWN && spawnAttackCoolDownTime >= spawnAttackCoolDownDuration
                    || currentAttackType is YaghotepAttackType.FORMATION && formationAttackCoolDownTime >= formationAttackCoolDownDuration)
                {
                    attackState = YaghotepAttackState.INIT;
                    return;
                }

                return;

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

    private void HandleSpawnAttackProcedure()
    {
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;
                navMeshAgent.isStopped = true;
                spawnAttackAnticipationTime = 0f;
                spawnAttackRecoveryTime = 0f;

                animator.SetTrigger("ChargeSpawn");
                break;
            case YaghotepAttackState.ANTICIPATION:
                var targetPosition = _selectedTarget.Center();
                var lookDirection = targetPosition - transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, transform.up);
                var newRotation = Quaternion.LookRotation(newForward, transform.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    newRotation,
                    ScaledDeltaTime * spawnAttackAnticipationRotationSpeed
                );

                var targetDirectionForPitch = targetPosition - spawnProjectileSpawnPoint.position;
                var horizontalDistance = new Vector3(targetDirectionForPitch.x, 0, targetDirectionForPitch.z).magnitude;
                var verticalDistance = targetDirectionForPitch.y;
                var desiredPitchAngle = -Mathf.Atan2(verticalDistance, horizontalDistance) * Mathf.Rad2Deg;
                var currentSpawnPointEuler = spawnProjectileSpawnPoint.localEulerAngles;
                var clampedDesiredPitch = Mathf.Clamp(desiredPitchAngle, -80f, 80f);
                var targetProjectileSpawnPointRotation = Quaternion.Euler(
                    clampedDesiredPitch - spawnAttackAdditionalPitch,
                    currentSpawnPointEuler.y,
                    currentSpawnPointEuler.z
                );
                spawnProjectileSpawnPoint.localRotation = Quaternion.RotateTowards(
                    spawnProjectileSpawnPoint.localRotation,
                    targetProjectileSpawnPointRotation,
                    ScaledDeltaTime * spawnAttackAnticipationRotationSpeed
                );

                spawnAttackAnticipationTime += ScaledDeltaTime;
                if (spawnAttackAnticipationTime >= spawnAttackAnticipationDuration)
                {
                    attackState = YaghotepAttackState.RECOVERY;
                    animator.SetTrigger("Recover");
                    var yRotation = -spawnAttackSpreadAngle;
                    var yRotationStep = (2f * spawnAttackSpreadAngle) / (spawnAttackProjectileCount - 1);
                    for (int i = 0; i < spawnAttackProjectileCount; i++)
                    {
                        Quaternion projectileOrientation = Quaternion.Euler(0f, yRotation, 0f) * spawnProjectileSpawnPoint.rotation;
                        var projectileObject = spawnAttackprojectilePool.GetObject();
                        var projectileTransform = projectileObject.transform;
                        projectileTransform.position = spawnProjectileSpawnPoint.position;
                        projectileTransform.rotation = projectileOrientation;
                        var projectileComponent = projectileObject.GetComponent<YaghotepSpawnProjectile>();
                        projectileObject.SetActive(true);
                        projectileComponent.Shoot(spawnAttackSpeed, spawnAttackGravityScale, OnEnemySpawned);
                        yRotation += yRotationStep;
                    }
                }
                break;
            case YaghotepAttackState.RECOVERY:
                spawnAttackRecoveryTime += ScaledDeltaTime;
                if (spawnAttackRecoveryTime >= spawnAttackRecoveryDuration)
                {
                    attackState = YaghotepAttackState.NONE;
                    spawnAttackCoolDownTime = 0f;
                    SetupStoppingDistanceState(stoppingDistanceState, navMeshAgent.destination);
                }
                break;
            case YaghotepAttackState.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }

    private void HandleFormationAttackProcedure()
    {
        Vector3 targetPosition, lookDirection, newForward;
        Quaternion newRotation;
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;

                formationAttackAnticipationDuration = formationChargeAnimationClip.length;
                navMeshAgent.isStopped = true;
                formationAttackAnticipationTime = 0f;
                formationAttackRecoveryTime = 0f;
                formationAttackExecutionTime = 0f;
                formationAttackExecutionCount = 0;
                animator.SetTrigger("ChargeFormation");
                break;
            case YaghotepAttackState.ANTICIPATION:
                targetPosition = _selectedTarget.Center();
                lookDirection = targetPosition - transform.position;
                newForward = Vector3.ProjectOnPlane(lookDirection, transform.up);
                newRotation = Quaternion.LookRotation(newForward, transform.up);
                transform.rotation = newRotation;

                formationAttackAnticipationTime += ScaledDeltaTime;

                if (formationAttackAnticipationTime >= formationAttackAnticipationDuration)
                {
                    var clipLength = formationExecutionAnimationClip.length;
                    formationAttackExecutionDuration = clipLength * formationAttackExecutionTotalNumber;
                    attackState = YaghotepAttackState.EXECUTE;
                }
                break;
            case YaghotepAttackState.EXECUTE:
                formationAttackExecutionTime += ScaledDeltaTime;
                var oldExecutionCount = formationAttackExecutionCount;
                var newExecutionCount = 1 + (int) (formationAttackExecutionTotalNumber * (formationAttackExecutionTime / formationAttackExecutionDuration));
                formationAttackExecutionCount = newExecutionCount;
                if (newExecutionCount == oldExecutionCount)
                {
                    return;
                }
                else if (formationAttackExecutionCount > formationAttackExecutionTotalNumber)
                {
                    attackState = YaghotepAttackState.RECOVERY;
                    animator.SetTrigger("Recover");
                    return;
                }

                targetPosition = _selectedTarget.Center();
                lookDirection = targetPosition - transform.position;
                newForward = Vector3.ProjectOnPlane(lookDirection, transform.up);
                newRotation = Quaternion.LookRotation(newForward, transform.up);
                transform.rotation = newRotation;
                var forward = (targetPosition - formationProjectileSpawnPoint.position);
                forward.y = 0;
                if (forward == Vector3.zero) forward = formationProjectileSpawnPoint.forward;
                forward.Normalize();

                // Evenly distribute projectiles in a circle
                float angleStep = 360f / formationAttackProjectileCount;
                var right = Vector3.Cross(forward, Vector3.up).normalized;

                Debug.DrawRay(formationProjectileSpawnPoint.position, Vector3.up * (-10f));

                for (int i = 0; i < formationAttackProjectileCount; i++)
                {
                    float angle = i * angleStep;
                    var projectilePosition = formationProjectileSpawnPoint.position
                        + Mathf.Sin(angle * Mathf.Deg2Rad) * formationRadius * Vector3.up
                        + Mathf.Cos(angle * Mathf.Deg2Rad) * formationRadius * right;

                    Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
                    var projectileObject = formationAttackprojectilePool.GetObject();
                    var projectileTransform = projectileObject.transform;
                    projectileTransform.position = projectilePosition;
                    projectileTransform.rotation = rotation;
                    var projectileComponent = projectileObject.GetComponent<YaghotepProjectile>();
                    projectileObject.SetActive(true);
                    projectileComponent.Shoot(formationAttackSpeed, formationAttackGravityScale);
                }
                break;
            case YaghotepAttackState.RECOVERY:
                formationAttackRecoveryTime += ScaledDeltaTime;
                if (formationAttackRecoveryTime >= formationAttackRecoveryDuration)
                {
                    attackState = YaghotepAttackState.NONE;
                    formationAttackCoolDownTime = 0f;
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
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.FOLLOWING, _selectedTarget.Center());
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.STAYING
            && distanceToTarget < stoppingDistancePushBack)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.FOLLOWING
            && distanceToTarget < stoppingDistanceStay
            || stoppingDistanceState == YaghotepStoppingDistanceState.PUSHED
            && distanceToTarget > stoppingDistanceStay
        )
        {
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.STAYING, _selectedTarget.Center());
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            SetDestination(fleeDestination);
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.FOLLOWING)
        {
            SetDestination(_selectedTarget.Center());
        }
    }

    private void SetupStoppingDistanceState(YaghotepStoppingDistanceState state, Vector3 destination)
    {
        switch (state)
        {
            case YaghotepStoppingDistanceState.PUSHED:
                navMeshAgent.isStopped = false;
                navMeshAgent.stoppingDistance = 0;
                animator.SetFloat("Speed", -1f);
                break;
            case YaghotepStoppingDistanceState.STAYING:
                navMeshAgent.stoppingDistance = stoppingDistanceFollow;
                navMeshAgent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                break;
            case YaghotepStoppingDistanceState.FOLLOWING:
                stoppingDistanceState = YaghotepStoppingDistanceState.FOLLOWING;
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

    public virtual void Hit(int damage)
    {
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
        foreach (var enemy in spawnedEnemies)
        {
            var enemyDied = enemy.GetComponent<OnEnemyDied>();
            if (enemyDied != null)
            {
                enemyDied.OnEnemyDiedEvent -= OnEnemyDied;
            }
        }
    }

    public void HitByPlayerFrom(Vector3 hitDirection)
    {
        if (currentState is YaghotepState.DYING or YaghotepState.AGGRESSIVE)
        {
            return;
        }

        checkingDestination = transform.position - hitDirection;
        SwitchState(YaghotepState.CHECKING);
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        spawnedEnemies.Add(enemy);
        var enemyDied = enemy.GetComponent<OnEnemyDied>();
        if (enemyDied != null)
        {
            enemyDied.OnEnemyDiedEvent += OnEnemyDied;
        }
    }

    private void OnEnemyDied(GameObject enemy)
    {
        spawnedEnemies.Remove(enemy);
        var enemyDied = enemy.GetComponent<OnEnemyDied>();
        if (enemyDied != null)
        {
            enemyDied.OnEnemyDiedEvent -= OnEnemyDied;
        }
    }

    private void SetDestination(Vector3 position)
    {
        navMeshDestination.position = position;
        navMeshAgent.destination = position;
    }
}
