using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class YaghotepJumpAttack
{
    public Animator animator;
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    public float ScaledDeltaTime;
    public float jumpAnticipationTime;
    public float jumpAnticipationDuration;
    public float jumpAnticipationRotationSpeed;
    public float jumpExecutionTime;
    public float jumpExecutionDuration;
    public float jumpRecoveryTime;
    public float jumpRecoveryDuration;
    public float jumpPower;
    public float jumpArcHeight;
    public Transform jumpStartPoint;
    public Transform jumpEndPoint;
    public Hittable _selectedTarget;
    public YaghotepAttackState attackState;
    public System.Action<YaghotepAttackState> SetAttackState;
    public GameObject pulseWavePrefab;
    public LayerMask groundLayer;
    public float landingCheckDistance = 0.5f;
    public Vector3 _jumpStartPos;
    public Vector3 _jumpEndPos;
    public float _jumpTotalTime;
    public bool _hasLanded;
    public void Init(Animator animator, NavMeshAgent navMeshAgent)
    {
        this.animator = animator;
        this.navMeshAgent = navMeshAgent;
    }

    public void HandleJumpAttackProcedure()
    {
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;
                navMeshAgent.isStopped = true;
                jumpAnticipationTime = 0f;
                jumpRecoveryTime = 0f;
                jumpExecutionTime = 0f;
                _hasLanded = false;
                animator.SetTrigger("ChargeJump");
                _jumpStartPos = navMeshAgent.transform.position;
                _jumpEndPos = _selectedTarget.Center();
                _jumpTotalTime = jumpExecutionDuration;
                break;
            case YaghotepAttackState.ANTICIPATION:
                var targetPosition = _selectedTarget.Center();
                var lookDirection = targetPosition - navMeshAgent.transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, navMeshAgent.transform.up);
                var newRotation = Quaternion.LookRotation(newForward, navMeshAgent.transform.up);
                navMeshAgent.transform.rotation = Quaternion.RotateTowards(
                    navMeshAgent.transform.rotation,
                    newRotation,
                    ScaledDeltaTime * jumpAnticipationRotationSpeed
                );
                jumpAnticipationTime += ScaledDeltaTime;
                if (jumpAnticipationTime >= jumpAnticipationDuration)
                {
                    attackState = YaghotepAttackState.EXECUTE;
                    animator.SetTrigger("Jump");
                }
                break;
            case YaghotepAttackState.EXECUTE:
                jumpExecutionTime += ScaledDeltaTime;
                float t = Mathf.Clamp01(jumpExecutionTime / _jumpTotalTime);
                // Ballistic arc (parabola) between start and end
                Vector3 jumpPos = Vector3.Lerp(_jumpStartPos, _jumpEndPos, t);
                float arc = Mathf.Sin(Mathf.PI * t) * jumpArcHeight;
                jumpPos.y += arc;
                navMeshAgent.transform.position = jumpPos;
                // Check for landing (raycast down)
                if (!_hasLanded && t >= 1f)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(navMeshAgent.transform.position, Vector3.down, out hit, landingCheckDistance, groundLayer))
                    {
                        _hasLanded = true;
                        // Spawn pulse wave
                        if (pulseWavePrefab)
                        {
                            GameObject.Instantiate(pulseWavePrefab, hit.point, Quaternion.identity);
                        }
                        attackState = YaghotepAttackState.RECOVERY;
                        animator.SetTrigger("Recover");
                    }
                }
                break;
            case YaghotepAttackState.RECOVERY:
                jumpRecoveryTime += ScaledDeltaTime;
                if (jumpRecoveryTime >= jumpRecoveryDuration)
                {
                    attackState = YaghotepAttackState.NONE;
                    SetAttackState?.Invoke(YaghotepAttackState.NONE);
                }
                break;
            case YaghotepAttackState.NONE:
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }
} 