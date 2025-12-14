using F3PS.AI.Sensors;
using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class YaghotepJumpAttack
{
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;

    [Header("Debug")]
    public YaghotepAttackState attackState;
    public SensorController _sensorController;
    public bool _hasLanded;
    public float scaledDeltaTime;
    public float anticipationTime;
    public float recoveryTime;
    public Vector3 startPos;
    public Vector3 endPos;
    public Vector3 landingPosition;
    public float timeScale = 1f;

    [Space(10)]
    [Header("References")]
    public DonutShockwave donutShockwave;
    public GameObject pulseWavePrefab;
    public LayerMask groundLayer;
    public AnimationClip anticipationAnimationClip;
    public AnimationClip recoveryAnimationClip;

    [Space(10)]
    [Header("Settings")]
    public float jumpDistance;
    public float jumpHeight;
    public float attackDistance;
    public float anticipationRotationSpeed;
    [Space(10)]
    public float coolDownTime;
    public float coolDownDuration;
    [Space(10)]
    public float jumpUpTime;
    public float jumpUpDuration;
    [Space(10)]
    public float stayInMidAirTime;
    public float stayInMidAirDuration;
    [Space(10)]
    public float fallTime;
    public float fallDuration;
    [Space(10)]
    public int shockWaveDamage;
    public float shockWaveExpansionSpeed;
    public float shockWaveThickness;
    public float shockWaveMaxRadius;


    public void Init(
        SensorController sensorController,
        Animator animator,
        NavMeshAgent navMeshAgent,
        Collider[] collidersToIgnore
    )
    {
        _sensorController = sensorController;
        _animator = animator;
        _navMeshAgent = navMeshAgent;
        _transform = navMeshAgent.transform;
        donutShockwave.Init(collidersToIgnore);
    }

    public void HandleJumpAttackProcedure()
    {
        switch (attackState)
        {
            case YaghotepAttackState.INIT:
                attackState = YaghotepAttackState.ANTICIPATION;
                _navMeshAgent.isStopped = true;
                _navMeshAgent.updatePosition = false;
                _navMeshAgent.updateRotation = false;
                anticipationTime = 0f;
                recoveryTime = 0f;
                jumpUpTime = 0f;
                fallTime = 0f;
                stayInMidAirTime = 0f;
                _hasLanded = false;
                _animator.SetTrigger("ChargeJump");
                break;
            case YaghotepAttackState.ANTICIPATION:
                var targetPosition = _sensorController.GetTargetFromSensors().Center();
                var lookDirection = targetPosition - _transform.position;
                var newForward = Vector3.ProjectOnPlane(lookDirection, _transform.up);
                var newRotation = Quaternion.LookRotation(newForward, _transform.up);
                _transform.rotation = Quaternion.RotateTowards(
                    _transform.rotation,
                    newRotation,
                    scaledDeltaTime * anticipationRotationSpeed
                );
                anticipationTime += scaledDeltaTime;
                if (anticipationTime >= anticipationAnimationClip.length)
                {
                    startPos = _transform.position;
                    endPos = startPos + _transform.forward * jumpDistance + Vector3.up * jumpHeight;
                    attackState = YaghotepAttackState.EXECUTE;
                    Physics.Raycast(endPos, Vector3.down, out var hit, 100f, groundLayer);
                    landingPosition = hit.point;
                    _animator.SetTrigger("Jump");
                    _animator.SetFloat("JumpVelocity", 1f);
                }
                break;
            case YaghotepAttackState.EXECUTE:
                if (jumpUpTime < jumpUpDuration)
                {
                    var nextPosition = new Vector3(
                        Mathf.Lerp(startPos.x, endPos.x, jumpUpTime / jumpUpDuration),
                        Mathf.Lerp(startPos.y, endPos.y, Helper.Easing.EaseOutQuad(jumpUpTime / jumpUpDuration)),
                        Mathf.Lerp(startPos.z, endPos.z, jumpUpTime / jumpUpDuration)
                    );
                    _transform.position = nextPosition;
                    jumpUpTime += scaledDeltaTime;
                    _animator.SetFloat("JumpVelocity", 1f - (jumpUpTime / jumpUpDuration) * 0.9f + 0.1f);
                }
                else if (jumpUpTime >= jumpUpDuration && stayInMidAirTime == 0f)
                {
                    _transform.position = endPos;
                    stayInMidAirTime += scaledDeltaTime;
                    _animator.SetFloat("JumpVelocity", 0.1f - (stayInMidAirTime / stayInMidAirDuration) * 0.2f);
                }
                else if (stayInMidAirTime < stayInMidAirDuration)
                {
                    stayInMidAirTime += scaledDeltaTime;
                    _animator.SetFloat("JumpVelocity", 0.1f - (stayInMidAirTime / stayInMidAirDuration) * 0.2f);
                }
                else if (stayInMidAirTime >= stayInMidAirDuration && fallTime == 0f)
                {
                    fallTime += scaledDeltaTime;
                    _animator.SetFloat("JumpVelocity", -0.1f - (fallTime / fallDuration) * 0.9f);
                }
                else if (fallTime < fallDuration)
                {
                    var nextPosition = new Vector3(
                        landingPosition.x,
                        Mathf.Lerp(endPos.y, landingPosition.y, Helper.Easing.EaseInQuad(fallTime/ fallDuration)),
                        landingPosition.z
                    );
                    _transform.position = nextPosition;
                    fallTime += scaledDeltaTime;
                    _animator.SetFloat("JumpVelocity", -0.1f - (fallTime / fallDuration) * 0.9f);
                }
                else if (fallTime >= fallDuration)
                {
                    // Spawn shockwave
                    donutShockwave.StartShockwave(
                        landingPosition,
                        shockWaveDamage,
                        shockWaveExpansionSpeed,
                        shockWaveThickness,
                        shockWaveMaxRadius,
                        timeScale
                    );
                    _transform.position = landingPosition;
                    attackState = YaghotepAttackState.RECOVERY;
                    _animator.SetTrigger("Recover");
                }
                break;
            case YaghotepAttackState.RECOVERY:
                recoveryTime += scaledDeltaTime;
                if (recoveryTime >= recoveryAnimationClip.length)
                {
                    attackState = YaghotepAttackState.NONE;
                    _navMeshAgent.Warp(landingPosition);
                    _navMeshAgent.updatePosition = true;
                    _navMeshAgent.updateRotation = true;
                    coolDownTime = 0f;
                }
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(attackState), attackState, null);
        }
    }

    public bool IsAttackReady()
    {
        return coolDownTime >= coolDownDuration;
    }

    public void UpdateCoolDown()
    {
        if (attackState == YaghotepAttackState.NONE)
        {
            coolDownTime += scaledDeltaTime;
        }
    }
} 