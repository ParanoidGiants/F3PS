using F3PS;
using StarterAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum RotationCommand
{
    None = 0,
    RotateRight,
    RotateLeft,
    RotateUp,
    RotateDown
}

public class ThotMindController : MonoBehaviour
{
    private ThotMindSkillData ThotMindSkillData => GameManager.Instance.PlayerData.ThotMindSkillData;
    private const float ROTATION_INPUT_THRESHOLD = 0.3f;

    [Header("References")]
    public Crosshair crosshair;
    public LayerMask movableLayer;
    public LineRenderer lineRenderer;
    public Transform target;
    public Transform playerCameraTransform;
    public Animator animator;

    [Space(10)]
    [Header("Watchers")]
    public ThotMindMovable currentCandidate;
    public Vector3 targetPosition;

    public bool wasObjectTouchedByPlayer = false;

    public bool isActuallyMovingObject = false;
    public bool wasMovingObjectLastFrame = false;
    public bool isMovingObjectThisFrame = false;

    public bool isActuallyRotatingObject = false;
    public bool wasRotatingObjectLastFrame = false;
    public bool isRotatingObjectThisFrame = false;

    public bool hasCandidate = false;

    private static List<Quaternion> uniqueRotations = new List<Quaternion>();
    public Quaternion SubjectOrientation
    {
        get {
            var forward = playerCameraTransform.forward;
            var projectedForward = new Vector3(forward.x, 0f, forward.z);
            return Quaternion.LookRotation(projectedForward, Vector3.up);
        }
    }

    private void Awake()
    {
        InitializeRotations();
    }

    public void OnDisable()
    {
        target.gameObject.SetActive(false);
        if (!hasCandidate)
        {
            return;
        }

        if (isActuallyMovingObject)
        {
            currentCandidate.StopMoving(ThotMindSkillData.MaximumThrowSpeed);
        }


        currentCandidate.UnselectAsCandidate();
        isActuallyMovingObject = false;
        animator.SetBool("ThotMind", false);
        isRotatingObjectThisFrame = false;
        hasCandidate = false;
    }

    public void OnUpdate(bool isMoving, bool isRotating, Vector2 rotationDeltaXY, float pushPull)
    {
        wasMovingObjectLastFrame = isMovingObjectThisFrame ;
        isMovingObjectThisFrame = isMoving;

        if (!PlayerIsGrounded() || !hasCandidate || currentCandidate.IsLocked)
        {
            isActuallyMovingObject = false;
            animator.SetBool("ThotMind", false);
            return;
        }

        var startMovingObject = !wasMovingObjectLastFrame && isMovingObjectThisFrame;
        var stopMovingObject = wasMovingObjectLastFrame && !isMovingObjectThisFrame;
        var isTouchedByPlayer = currentCandidate.isCurrentlyTouchedByPlayer;

        if (isTouchedByPlayer)
        {
            if (isActuallyMovingObject)
            {
                isActuallyMovingObject = false;
                animator.SetBool("ThotMind", false);
                isActuallyRotatingObject = false;
                target.gameObject.SetActive(false);
                currentCandidate.StopMoving(ThotMindSkillData.MaximumThrowSpeed);
            }
            wasObjectTouchedByPlayer = true;
            currentCandidate.SetLocked();
            return;
        }
        else if (wasObjectTouchedByPlayer)
        {
            wasObjectTouchedByPlayer = false;
            currentCandidate.SetUnpicked();
        }

        if (startMovingObject)
        {
            isActuallyMovingObject = true;
            animator.SetBool("ThotMind", true);
            target.gameObject.SetActive(true);
            SetTargetPosition(targetPosition);
            currentCandidate.SnapToRelativeRotation(SubjectOrientation);
            currentCandidate.StartMoving();
            return;
        }
        
        if (stopMovingObject)
        {
            // Stop Moving Object
            isActuallyMovingObject = false;
            animator.SetBool("ThotMind", false);
            isActuallyRotatingObject = false;
            target.gameObject.SetActive(false);
            currentCandidate.StopMoving(ThotMindSkillData.MaximumThrowSpeed);
            return;
        }

        if (!isActuallyMovingObject)
        {
            return;
        }


        if (pushPull != 0f)
        {
            var moveDirection = target.forward;
            var moveTarget = target.position + pushPull * ThotMindSkillData.PushPullSpeed * Time.deltaTime * moveDirection;
            SetTargetPosition(moveTarget);
        }

        wasRotatingObjectLastFrame = isRotatingObjectThisFrame;
        isRotatingObjectThisFrame = isRotating;
        if (isActuallyRotatingObject)
        {
            var rotationCommand = GetRotationCommand(rotationDeltaXY);
            currentCandidate.Rotate(rotationCommand, SubjectOrientation, ThotMindSkillData.RotateTimer);
        }
        var startRotatingObject = !wasRotatingObjectLastFrame && isRotatingObjectThisFrame;
        var stopRotatingObject = wasRotatingObjectLastFrame && !isRotatingObjectThisFrame;
        if (startRotatingObject)
        {
            isActuallyRotatingObject = true;
            currentCandidate.StartRotating();
        }
        else if (stopRotatingObject)
        {
            isActuallyRotatingObject = false;
            currentCandidate.StopRotating();
        }
        else if (!currentCandidate.isBeingRotated)
        {
            currentCandidate.SnapToRelativeRotation(SubjectOrientation);
        }
    }

    private bool PlayerIsGrounded()
    {
        return FindFirstObjectByType<ThirdPersonController>().IsGrounded;
    }

    public void OnFixedUpdate()
    {
        if (hasCandidate && isActuallyMovingObject)
        {
            targetPosition = target.position;
            currentCandidate.MoveTowards(targetPosition, ThotMindSkillData.MoveSpeed, transform.right);
            return;
        }
        if (!crosshair.CrosshairRaycast())
        {
            targetPosition = crosshair.GetInfiniteDirection();

            if (hasCandidate)
            {
                hasCandidate = false;
                currentCandidate.UnselectAsCandidate();
                currentCandidate = null;
            }
            return;
        }

        var targetHit = crosshair.Target;
        var targetTransform = targetHit.transform;
        var ray = crosshair.Ray;
        var movableTarget = targetTransform.GetComponent<ThotMindMovable>();

        if (movableTarget == null)
        {
            if (hasCandidate)
            {
                hasCandidate = false;
                currentCandidate.UnselectAsCandidate();
                currentCandidate = null;
            }
            targetPosition = crosshair.GetInfiniteDirection();
            return;
        }

        targetPosition = MathUtils.ClosestPointOnRay(ray, targetTransform.position);
        if (hasCandidate)
        {
            if (movableTarget == currentCandidate)
            {
                return;
            }
            else
            {
                currentCandidate.UnselectAsCandidate();
                InitializeCandidate(movableTarget);
                currentCandidate = movableTarget;
            }
        }
        else if (!hasCandidate)
        {
            hasCandidate = true;
            InitializeCandidate(movableTarget);
            currentCandidate = movableTarget;
        }
    }

    private void InitializeCandidate(ThotMindMovable movableTarget)
    {
        movableTarget.SelectAsCandidate();
        if (movableTarget.IsLocked)
        {
            movableTarget.SetLocked();
        }
        else
        {
            movableTarget.SetUnpicked();
        }
    }

    private void SetTargetPosition(Vector3 position)
    {
        target.position = position;
        if (target.localPosition.z >= ThotMindSkillData.MinimumDistance && target.localPosition.z <= ThotMindSkillData.MaximumDistance)
        {
            return;
        }
        var clamped = Mathf.Clamp(target.localPosition.z, ThotMindSkillData.MinimumDistance, ThotMindSkillData.MaximumDistance);
        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, clamped);
    }

    private RotationCommand GetRotationCommand(Vector2 rotationDeltaXY)
    {
        var x = rotationDeltaXY.x;
        var y = rotationDeltaXY.y;
        if (Mathf.Abs(x) < ROTATION_INPUT_THRESHOLD || Mathf.Abs(y) < ROTATION_INPUT_THRESHOLD)
            return RotationCommand.None;

        if (Mathf.Abs(x) < Mathf.Abs(y))
        {
            return y < 0 ? RotationCommand.RotateUp : RotationCommand.RotateDown;
        }
        else
        {
            return x < 0 ? RotationCommand.RotateLeft : RotationCommand.RotateRight;
        }
    }
    private void InitializeRotations()
    {
        int divisions = 8;
        float step = 360f / divisions;

        for (int x = 0; x < divisions; x++)
        {
            for (int y = 0; y < divisions; y++)
            {
                for (int z = 0; z < divisions; z++)
                {
                    var xRot = x * step - 180f;
                    var yRot = y * step - 180f;
                    var zRot = z * step - 180f;
                    Quaternion q = Quaternion.Euler(xRot, yRot, zRot);
                    if (!uniqueRotations.Any(existing => Quaternion.Dot(q, existing) > 0.9999f))
                    {
                        uniqueRotations.Add(q);
                    }
                }
            }
        }
    }

    public static Quaternion GetClosestRotation(Quaternion target)
    {
        float maxDot = -1f;
        Quaternion closest = Quaternion.identity;

        foreach (var q in uniqueRotations)
        {
            float dot = Mathf.Abs(Quaternion.Dot(target, q));
            if (dot > maxDot)
            {
                maxDot = dot;
                closest = q;
            }
        }

        return closest;
    }

    public static Quaternion GetStepRotationByRotationCommand(RotationCommand command)
    {
        float step = 45f;
        switch (command)
        {
            case RotationCommand.RotateRight:
                return Quaternion.Euler(0, -step, 0);
            case RotationCommand.RotateLeft:
                return Quaternion.Euler(0, step, 0);
            case RotationCommand.RotateUp:
                return Quaternion.Euler(step, 0, 0);
            case RotationCommand.RotateDown:
                return Quaternion.Euler(-step, 0, 0);
            default:
                return Quaternion.identity;
        }
    }

    internal void OnLateUpdate()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetPosition);
    }
}
