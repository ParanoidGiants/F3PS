using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class YaghotepSpawnMinionsAttack
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    [Header("Debug")]
    public List<GameObject> spawnedMinions; 
    public YaghotepAttackState attackState;
    public Hittable _selectedTarget;
    public float scaledDeltaTime;
    public float anticipationTime;
    public float recoveryTime;
    public float coolDownTime;

    [Space(10)]
    [Header("References")]
    public Transform projectileSpawnPoint;
    public ObjectPool projectilePool;
    public AnimationClip anticipationAnimationClip;
    public AnimationClip recoveryAnimationClip;

    [Space(10)]
    [Header("Settings")]
    public float anticipationRotationSpeed;
    public float coolDownDuration;
    public float projectileSpreadAngle;
    public float projectileSpeed;
    public float projectileGravityScale;
    public float additionalShootPitch;
    public int projectileCount;
    public int maximumMinionCount;

    public void Init(Transform parent, Collider[] collidersThatShouldntBeHit, Animator animator, NavMeshAgent navMeshAgent)
    {
        _animator = animator;
        _navMeshAgent = navMeshAgent;

        projectilePool.Init(parent);

        var spawnProjectiles = projectilePool.GetObjects();
        foreach (var projectile in spawnProjectiles)
        {
            var projectileComponent = projectile.GetComponent<YaghotepSpawnProjectile>();
            projectileComponent.Init(parent.gameObject, collidersThatShouldntBeHit);
            projectile.SetActive(false);
        }
    }

    public void HandleSpawnAttackProcedure()
    {
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;
                _navMeshAgent.isStopped = true;
                anticipationTime = 0f;
                recoveryTime = 0f;
                _animator.SetTrigger("ChargeSpawn");
                break;
            case YaghotepAttackState.ANTICIPATION:
                var targetPosition = _selectedTarget.Center();
                var lookDirection = targetPosition - _navMeshAgent.transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, _navMeshAgent.transform.up);
                var newRotation = Quaternion.LookRotation(newForward, _navMeshAgent.transform.up);
                _navMeshAgent.transform.rotation = Quaternion.RotateTowards(
                    _navMeshAgent.transform.rotation,
                    newRotation,
                    scaledDeltaTime * anticipationRotationSpeed
                );
                var targetDirectionForPitch = targetPosition - projectileSpawnPoint.position;
                var horizontalDistance = new Vector3(targetDirectionForPitch.x, 0, targetDirectionForPitch.z).magnitude;
                var verticalDistance = targetDirectionForPitch.y;
                var desiredPitchAngle = -Mathf.Atan2(verticalDistance, horizontalDistance) * Mathf.Rad2Deg;
                var currentSpawnPointEuler = projectileSpawnPoint.localEulerAngles;
                var clampedDesiredPitch = Mathf.Clamp(desiredPitchAngle, -80f, 80f);
                var targetProjectileSpawnPointRotation = Quaternion.Euler(
                    clampedDesiredPitch - additionalShootPitch,
                    currentSpawnPointEuler.y,
                    currentSpawnPointEuler.z
                );
                projectileSpawnPoint.localRotation = Quaternion.RotateTowards(
                    projectileSpawnPoint.localRotation,
                    targetProjectileSpawnPointRotation,
                    scaledDeltaTime * anticipationRotationSpeed
                );
                anticipationTime += scaledDeltaTime;
                if (anticipationTime >= anticipationAnimationClip.length)
                {
                    attackState = YaghotepAttackState.RECOVERY;
                    _animator.SetTrigger("Recover");
                    var yRotation = -projectileSpreadAngle;
                    var yRotationStep = (2f * projectileSpreadAngle) / (projectileCount - 1);
                    for (int i = 0; i < projectileCount; i++)
                    {
                        Quaternion projectileOrientation = Quaternion.Euler(0f, yRotation, 0f) * projectileSpawnPoint.rotation;
                        var projectileObject = projectilePool.GetObject();
                        var projectileTransform = projectileObject.transform;
                        projectileTransform.position = projectileSpawnPoint.position;
                        projectileTransform.rotation = projectileOrientation;
                        var projectileComponent = projectileObject.GetComponent<YaghotepSpawnProjectile>();
                        projectileObject.SetActive(true);
                        projectileComponent.Shoot(projectileSpeed, projectileGravityScale, OnEnemySpawned);
                        yRotation += yRotationStep;
                    }
                }
                break;
            case YaghotepAttackState.RECOVERY:
                recoveryTime += scaledDeltaTime;
                if (recoveryTime >= recoveryAnimationClip.length)
                {
                    attackState = YaghotepAttackState.NONE;
                    coolDownTime = 0f;
                }
                break;
            case YaghotepAttackState.NONE:
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        spawnedMinions.Add(enemy);
        var enemyDied = enemy.GetComponent<OnEnemyDied>();
        if (enemyDied != null)
        {
            enemyDied.OnEnemyDiedEvent += OnEnemyDied;
        }
    }

    private void OnEnemyDied(GameObject enemy)
    {
        spawnedMinions.Remove(enemy);
        var enemyDied = enemy.GetComponent<OnEnemyDied>();
        if (enemyDied != null)
        {
            enemyDied.OnEnemyDiedEvent -= OnEnemyDied;
        }
    }

    public bool AreAllMinionsDead()
    {
        return spawnedMinions.Count == 0 && maximumMinionCount != 0;
    }

    public void UpdateCoolDown()
    {
        if (attackState == YaghotepAttackState.NONE)
        {
            coolDownTime += scaledDeltaTime;
        }
    }

    public bool IsAttackReady()
    {
        return coolDownTime >= coolDownDuration;
    }

    public bool HasReachedMaximumMinions()
    {
        return spawnedMinions.Count >= maximumMinionCount;
    }

    internal void Died()
    {
        foreach (var enemy in spawnedMinions)
        {
            var enemyDied = enemy.GetComponent<OnEnemyDied>();
            if (enemyDied != null)
            {
                enemyDied.OnEnemyDiedEvent -= OnEnemyDied;
            }
        }
    }
} 