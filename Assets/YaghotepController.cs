using DarkTonic.MasterAudio;
using F3PS.AI.Sensors;
using F3PS.Enemy.UI;
using System;
using UnityEngine;
using UnityEngine.AI;

public enum YaghotepAttackPhase
{
    INTRO = 0,
    PROGRESSION = 1,
    TWIST = 2,
    CONCLUSION = 3
}

public enum YaghotepAttackType
{
    SPAWN,
    FORMATION,
    JUMP,
    NONE
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

    public YaghotepJumpAttack jumpAttack;

    [Header("Debug")]
    public Transform navMeshDestination;
    public EnemyHealthUIPool _healthUIPool;
    public YaghotepState currentState = YaghotepState.IDLE;
    public bool isDead = false;
    public Vector3 lastPosition;
    public bool debugIsStopped;
    public float debugStoppingDistance;
    public YaghotepAttackPhase currentAttackPhase = YaghotepAttackPhase.INTRO;

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
    public float aggressiveMoveSpeed = 0f;
    public float aggressiveRotationSpeed;
    public float stoppingDistancePushBack = 3f;
    public float stoppingDistanceStay = 3f;
    public float stoppingDistanceFollow = 1f;

    public YaghotepAttackType currentAttackType = YaghotepAttackType.SPAWN;
    public float attackProjectileDistance = 5f;

    [Header("Attacks")]
    public YaghotepSpawnMinionsAttack spawnAttack;
    public YaghotepFormationAttack formationAttack;

    protected void Awake()
    {
        var parent = transform.parent;
        spawnAttack.Init(
            parent,
            sensorController,
            collidersThatShouldntBeHit,
            animator,
            navMeshAgent
        );
        formationAttack.Init(
            parent,
            sensorController,
            collidersThatShouldntBeHit,
            animator,
            navMeshAgent,
            transform
        );

        jumpAttack.Init(
            sensorController,
            animator,
            navMeshAgent,
            collidersThatShouldntBeHit
        );
    }

    private void Start()
    {
        health = maxHealth;
        patrolManager.Init();
        EnterState(YaghotepState.IDLE);
        _healthUIPool = FindFirstObjectByType<EnemyHealthUIPool>();
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
                _healthUIPool.EnableBossUI();
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
        if (currentState is YaghotepState.AGGRESSIVE)
        {
            navMeshAgent.angularSpeed = 1000;
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


        if (currentState != YaghotepState.DYING && currentState != YaghotepState.AGGRESSIVE && sensorController.HasTarget())
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
                HandleAggressiveState();
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

    private void HandleAggressiveState()
    {
        var attackState = YaghotepAttackState.NONE;
        if (currentAttackType is YaghotepAttackType.SPAWN)
        {
            attackState = spawnAttack.attackState;
        }
        else if (currentAttackType is YaghotepAttackType.FORMATION)
        {
            attackState = formationAttack.attackState;
        }
        else if (currentAttackType is YaghotepAttackType.JUMP)
        {
            attackState = jumpAttack.attackState;
        }


        if (attackState is YaghotepAttackState.NONE && !sensorController.HasTarget())
        {
            SwitchState(YaghotepState.CHECKING);
            return;
        }

        if (attackState is not YaghotepAttackState.NONE)
        {
            if (currentAttackType is YaghotepAttackType.SPAWN)
            {
                spawnAttack.scaledDeltaTime = ScaledDeltaTime;
                spawnAttack.HandleSpawnAttackProcedure();
                return;
            }
            else if (currentAttackType is YaghotepAttackType.FORMATION)
            {
                formationAttack.scaledDeltaTime = ScaledDeltaTime;
                formationAttack.timeScale = TimeScale;
                formationAttack.HandleFormationAttackProcedure();
                return;
            }
            else if (currentAttackType is YaghotepAttackType.JUMP)
            {
                jumpAttack.scaledDeltaTime = ScaledDeltaTime;
                jumpAttack.timeScale = TimeScale;
                jumpAttack.HandleJumpAttackProcedure();
                return;
            }
            else
            {
                Debug.Log("No attack set?");
            }
        }


        var selectedTarget = sensorController.GetTargetFromSensors();
        var distanceToTarget = Helper.GetPathLengthOnNavMesh(transform.position, selectedTarget.Center());
        checkingDestination = selectedTarget.Center();

        HandleAggressiveStoppingDistance();

        DetermineAttackPhase();
        DetermineAttackType(distanceToTarget);


        switch (currentAttackType)
        {
            case YaghotepAttackType.SPAWN:
                spawnAttack.scaledDeltaTime = ScaledDeltaTime;
                spawnAttack.UpdateCoolDown();
                break;
            case YaghotepAttackType.FORMATION:
                formationAttack.scaledDeltaTime = ScaledDeltaTime;
                formationAttack.UpdateCoolDown();
                break;
            case YaghotepAttackType.JUMP:
                jumpAttack.scaledDeltaTime = ScaledDeltaTime;
                jumpAttack.UpdateCoolDown();
                break;
            case YaghotepAttackType.NONE:
                break;
            default:
                Debug.LogError($"Unknown attack type: {currentAttackType}");
                return;

        }

        var targetDirection = (selectedTarget.Center() - transform.position).normalized;
        bool isAlignedWithTarget = Helper.IsOrientedOnXZ(transform.forward, targetDirection, 0.01f);
        if (!isAlignedWithTarget)
        {
            return;
        }

        if (currentAttackType is YaghotepAttackType.SPAWN && spawnAttack.IsAttackReady())
        {
            spawnAttack.attackState = YaghotepAttackState.INIT;
        }
        else if (currentAttackType is YaghotepAttackType.FORMATION && formationAttack.IsAttackReady())
        {
            formationAttack.attackState = YaghotepAttackState.INIT;
        }
        else if (currentAttackType is YaghotepAttackType.JUMP && jumpAttack.IsAttackReady())
        {
            jumpAttack.attackState = YaghotepAttackState.INIT;
        }
    }

    private void DetermineAttackPhase()
    {
        if (health <= 0.25f * maxHealth && currentAttackPhase is YaghotepAttackPhase.TWIST)
        {
            currentAttackPhase = YaghotepAttackPhase.CONCLUSION;
            navMeshAgent.speed = 2f * aggressiveMoveSpeed;
            spawnAttack.projectileCount = 7;

            jumpAttack.jumpUpDuration = 0.5f;
            jumpAttack.stayInMidAirDuration = 0.25f;
            jumpAttack.fallDuration = 0.1f;
            jumpAttack.shockWaveExpansionSpeed = 30f;
            jumpAttack.attackDistance *= 2f;
            jumpAttack.jumpDistance *= 2f;
        }
        else if (health <= 0.5f * maxHealth && currentAttackPhase is YaghotepAttackPhase.PROGRESSION)
        {
            currentAttackPhase = YaghotepAttackPhase.TWIST;
            navMeshAgent.speed = 1.5f * aggressiveMoveSpeed;
            spawnAttack.projectileCount += 2;
        }
        else if (health <= 0.75f * maxHealth && currentAttackPhase is YaghotepAttackPhase.INTRO)
        {
            currentAttackPhase = YaghotepAttackPhase.PROGRESSION;
            navMeshAgent.speed = 1.25f * aggressiveMoveSpeed;
            spawnAttack.projectileCount += 1;
            spawnAttack.coolDownDuration -= 2f;
        }
    }

    private void DetermineAttackType(float distanceToTarget)
    {
        switch (currentAttackPhase)
        {
            case YaghotepAttackPhase.INTRO:
                if (!spawnAttack.HasReachedMaximumMinions() && currentAttackType is YaghotepAttackType.SPAWN
                    || spawnAttack.AreAllMinionsDead())
                {
                    currentAttackType = YaghotepAttackType.SPAWN;
                }
                else
                {
                    currentAttackType = YaghotepAttackType.NONE;
                }
                break;
            case YaghotepAttackPhase.PROGRESSION:
                if (distanceToTarget <= jumpAttack.attackDistance)
                {
                    currentAttackType = YaghotepAttackType.JUMP;
                }
                else if (!spawnAttack.HasReachedMaximumMinions() && currentAttackType is YaghotepAttackType.SPAWN
                    || spawnAttack.AreAllMinionsDead())
                {
                    currentAttackType = YaghotepAttackType.SPAWN;
                }
                else
                {
                    currentAttackType = YaghotepAttackType.NONE;
                }
                break;
            case YaghotepAttackPhase.TWIST:
                if (distanceToTarget <= jumpAttack.attackDistance)
                {
                    currentAttackType = YaghotepAttackType.JUMP;
                }
                else if (!spawnAttack.HasReachedMaximumMinions() && currentAttackType is YaghotepAttackType.SPAWN
                    || spawnAttack.AreAllMinionsDead())
                {
                    currentAttackType = YaghotepAttackType.SPAWN;
                }
                else
                {
                    currentAttackType = YaghotepAttackType.FORMATION;
                }
                break;
            case YaghotepAttackPhase.CONCLUSION:
                if (distanceToTarget <= jumpAttack.attackDistance)
                {
                    currentAttackType = YaghotepAttackType.JUMP;
                }
                else if (!spawnAttack.HasReachedMaximumMinions() && currentAttackType is YaghotepAttackType.SPAWN
                    || spawnAttack.AreAllMinionsDead())
                {
                    currentAttackType = YaghotepAttackType.SPAWN;
                }
                else
                {
                    currentAttackType = YaghotepAttackType.FORMATION;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentAttackPhase), currentAttackPhase, null);
        }
    }

    private void HandleAggressiveStoppingDistance()
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
        if (stoppingDistanceState == YaghotepStoppingDistanceState.STAYING
            && distanceToTarget > stoppingDistanceFollow)
        {
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.FOLLOWING, selectedTarget.Center());
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
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.STAYING, selectedTarget.Center());
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.PUSHED)
        {
            var fleeDirection = -lookDirection;
            var ray = new Ray(transform.position, fleeDirection);
            Physics.Raycast(ray, out var hit, stoppingDistanceStay);
            var distance = Mathf.Max(stoppingDistanceStay, hit.distance);
            var fleeDestination = transform.position + fleeDirection * distance;
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.PUSHED, fleeDestination);
        }
        else if (stoppingDistanceState == YaghotepStoppingDistanceState.FOLLOWING)
        {
            SetupStoppingDistanceState(YaghotepStoppingDistanceState.FOLLOWING, selectedTarget.Center());
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
            _healthUIPool.DisableBossUI();
            SwitchState(YaghotepState.DYING);
            return;
        }
        _healthUIPool.OnHitBoss(health, maxHealth);
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
        spawnAttack.Died();
        Destroy(gameObject);
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

    private void SetDestination(Vector3 position)
    {
        navMeshDestination.position = position;
        navMeshAgent.destination = position;
    }
}
