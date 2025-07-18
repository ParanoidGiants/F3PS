using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class YaghotepFormationAttack
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;

    [Header("Debug")]
    public Hittable _selectedTarget;
    public YaghotepAttackState attackState;
    public float scaledDeltaTime;
    public float anticipationTime;
    public float executionTime;
    public int executionCount;
    public float recoveryTime;
    public float coolDownTime;

    [Space(10)]
    [Header("References")]
    public Transform projectileSpawnPoint;
    public ObjectPool projectilePool;
    public AnimationClip anticipationAnimationClip;
    public AnimationClip executionAnimationClip;
    public AnimationClip recoveryAnimationClip;

    [Space(10)]
    [Header("Settings")]
    public float executionTotalNumber;
    public float coolDownDuration;
    public int projectileCount;
    public float formationRadius;
    public float projectileSpeed;
    public float projectileGravityScale;

    public void Init(Transform parent, Collider[] collidersThatShouldntBeHit, Animator animator, NavMeshAgent navMeshAgent, Transform transform)
    {
        _animator = animator;
        _navMeshAgent = navMeshAgent;
        _transform = transform;

        projectilePool.Init(parent);

        var formationProjectiles = projectilePool.GetObjects();
        foreach (var projectile in formationProjectiles)
        {
            var projectileComponent = projectile.GetComponent<YaghotepProjectile>();
            projectileComponent.Init(parent.gameObject, collidersThatShouldntBeHit);
            projectile.SetActive(false);
        }
    }

    public void HandleFormationAttackProcedure()
    {
        Vector3 targetPosition, lookDirection, newForward;
        Quaternion newRotation;
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;
                _navMeshAgent.isStopped = true;
                anticipationTime = 0f;
                recoveryTime = 0f;
                executionTime = 0f;
                executionCount = 0;
                _animator.SetTrigger("ChargeFormation");
                break;
            case YaghotepAttackState.ANTICIPATION:
                targetPosition = _selectedTarget.Center();
                lookDirection = targetPosition - _navMeshAgent.transform.position;
                newForward = Vector3.ProjectOnPlane(lookDirection, _navMeshAgent.transform.up);
                newRotation = Quaternion.LookRotation(newForward, _navMeshAgent.transform.up);
                _navMeshAgent.transform.rotation = newRotation;
                anticipationTime += scaledDeltaTime;
                if (anticipationTime >= anticipationAnimationClip.length)
                {
                    attackState = YaghotepAttackState.EXECUTE;
                }
                break;
            case YaghotepAttackState.EXECUTE:
                executionTime += scaledDeltaTime;
                var oldExecutionCount = executionCount;
                var executionDuration = executionAnimationClip.length * executionTotalNumber;
                var newExecutionCount = 1 + (int)(executionTotalNumber * (executionTime / executionDuration));
                executionCount = newExecutionCount;
                if (newExecutionCount == oldExecutionCount)
                {
                    return;
                }
                else if (executionCount > executionTotalNumber)
                {
                    attackState = YaghotepAttackState.RECOVERY;
                    _animator.SetTrigger("Recover");
                    return;
                }
                targetPosition = _selectedTarget.Center();
                lookDirection = targetPosition - _navMeshAgent.transform.position;
                newForward = Vector3.ProjectOnPlane(lookDirection, _navMeshAgent.transform.up);
                newRotation = Quaternion.LookRotation(newForward, _navMeshAgent.transform.up);
                _navMeshAgent.transform.rotation = newRotation;
                var forward = (targetPosition - projectileSpawnPoint.position);
                forward.y = 0;
                if (forward == Vector3.zero) forward = projectileSpawnPoint.forward;
                forward.Normalize();
                if (Vector3.Dot(forward, _transform.forward) < 0)
                {
                    forward = -forward;
                }

                float angleStep = 360f / projectileCount;
                var right = Vector3.Cross(forward, Vector3.up).normalized;
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = i * angleStep;
                    var projectilePosition = projectileSpawnPoint.position
                        + Mathf.Sin(angle * Mathf.Deg2Rad) * formationRadius * Vector3.up
                        + Mathf.Cos(angle * Mathf.Deg2Rad) * formationRadius * right;
                    Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
                    var projectileObject = projectilePool.GetObject();
                    var projectileTransform = projectileObject.transform;
                    projectileTransform.position = projectilePosition;
                    projectileTransform.rotation = rotation;
                    var projectileComponent = projectileObject.GetComponent<YaghotepProjectile>();
                    projectileObject.SetActive(true);
                    projectileComponent.Shoot(projectileSpeed, projectileGravityScale);
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
} 