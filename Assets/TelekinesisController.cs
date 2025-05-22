using Cinemachine;
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

public class TelekinesisController : MonoBehaviour
{
    private const float ROTATION_INPUT_THRESHOLD = 0.3f;

    [Header("References")]
    public Crosshair crosshair;
    public LayerMask movableLayer;
    public LineRenderer lineRenderer;
    public Transform target;
    public Transform playerCameraTransform;

    [Space(10)]
    [Header("Settings")]
    public float pushPullSpeed = 5.0f;
    public float rotateTimer = 1f;
    public float moveSpeed = 5.0f;
    public float minimumDistance = 3f;
    public float maximumDistance = 50f;
    public float maximumThrowSpeed = 3f;


    [Space(10)]
    [Header("Watchers")]
    public TelekinesisMovable currentCandidate;
    public Vector3 targetPosition;
    public bool isMovingObject = false;
    public bool isRotatingObject = false;
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

    private void Start()
    {
        InitializeRotations();
    }

    public void OnDisable()
    {
        StopTelekinesis();
    }

    public void OnUpdate(bool isMoving, bool isRotating, Vector2 rotationDeltaXY, float pushPull)
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetPosition);
        if (!hasCandidate)
        {
            return;
        }

        if (!isMovingObject && isMoving)
        {
            target.gameObject.SetActive(true);
            SetTargetPosition(targetPosition);

            currentCandidate.SnapToRelativeRotation(SubjectOrientation);
            currentCandidate.StartMoving();
            isMovingObject = true;
        }
        else if (isMovingObject && !isMoving)
        {
            StopTelekinesis();
        }
        else if (isMovingObject && !isRotatingObject && isRotating)
        {
            isRotatingObject = true;
            currentCandidate.StartRotating();
        }
        else if (isMovingObject && isRotatingObject && !isRotating)
        {
            isRotatingObject = false;
            currentCandidate.StopRotating();
        }
        else if (isMovingObject)
        {
            if (!currentCandidate.isInRotationCoroutine)
            {
                currentCandidate.SnapToRelativeRotation(SubjectOrientation);
            }
            if (pushPull != 0f)
            {
                var moveDirection = target.forward;
                var moveTarget = target.position + pushPull * pushPullSpeed * Time.deltaTime * moveDirection;

                SetTargetPosition(moveTarget);
            }
            if (isRotating)
            {
                var rotationCommand = GetRotationCommand(rotationDeltaXY);
                currentCandidate.Rotate(rotationCommand, SubjectOrientation, rotateTimer);
            }
        }
    }

    private void StopTelekinesis()
    {
        target.gameObject.SetActive(false);
        if (!hasCandidate)
        {
            return;
        }
        currentCandidate.StopMoving(maximumThrowSpeed);
        isMovingObject = false;
        isRotatingObject = false;
    }

    public void OnFixedUpdate()
    {
        if (isMovingObject)
        {
            targetPosition = target.position;
            currentCandidate.MoveTowards(targetPosition, moveSpeed, transform.right);
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
        var movableTarget = targetTransform.GetComponent<TelekinesisMovable>();

        var targetHasMovable = movableTarget != null;
        if (hasCandidate && !targetHasMovable)
        {
            hasCandidate = false;
            currentCandidate.UnselectAsCandidate();
            currentCandidate = null;
            targetPosition = targetHit.point;
            return;
        }
        else if (!targetHasMovable)
        {
            targetPosition = crosshair.GetInfiniteDirection();
            return;
        }

        targetPosition = MathUtils.ClosestPointOnRay(ray, targetTransform.position);
        if (hasCandidate && targetHasMovable)
        {
            if (movableTarget == currentCandidate)
            {
                return;
            }
            else
            {
                currentCandidate.UnselectAsCandidate();
                movableTarget.SelectAsCandidate();
                currentCandidate = movableTarget;
            }
        }
        else if (!hasCandidate && targetHasMovable)
        {
            hasCandidate = true;
            currentCandidate = movableTarget;
            currentCandidate.SelectAsCandidate();
        }
    }

    private void SetTargetPosition(Vector3 position)
    {
        target.position = position;
        if (target.localPosition.z >= minimumDistance && target.localPosition.z <= maximumDistance)
        {
            return;
        }
        var clamped = Mathf.Clamp(target.localPosition.z, minimumDistance, maximumDistance);
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
        Debug.Log("Unique Rotations Count: " + uniqueRotations.Count);
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
}
