using UnityEngine;

public class TelekinesisController : MonoBehaviour
{
    [Header("References")]
    public Crosshair crosshair;
    public LayerMask movableLayer;
    public LineRenderer lineRenderer;
    public Transform target;

    [Space(10)]
    [Header("Settings")]
    public float pushPullSpeed = 5.0f;
    public float moveSpeed = 5.0f;
    public float minimumDistance = 3f;
    public float maximumDistance = 50f;


    [Space(10)]
    [Header("Watchers")]
    public TelekinesisMovable currentCandidate;
    public Vector3 movablePoint;
    public bool isMovingObject = false;
    public bool hasCandidate = false;

    public void OnUpdate(bool isShooting, float pushPull)
    {
        if (!hasCandidate)
        {
            return;
        }

        if (!isMovingObject &&  isShooting)
        {
            target.gameObject.SetActive(true);
            SetTargetPosition(movablePoint);
            currentCandidate.StartMoving();
            isMovingObject = true;
        }
        else if (isMovingObject && !isShooting)
        {
            target.gameObject.SetActive(false);
            currentCandidate.StopMoving();
            isMovingObject = false;
        }
        else if (pushPull != 0f)
        {
            HandlePushPull(pushPull);
        }

    }

    private void SetTargetPosition(Vector3 position)
    {
        target.position = position;
        ClampTargetPosition();
    }

    private void HandlePushPull(float pushPull)
    {
        var moveDirection = target.forward;
        var moveTarget = target.position + pushPull * pushPullSpeed * Time.deltaTime * moveDirection;

        SetTargetPosition(moveTarget);
    }

    private void ClampTargetPosition()
    {
        if (target.localPosition.z >= minimumDistance && target.localPosition.z <= maximumDistance)
        {
            return;
        }
        var clamped = Mathf.Clamp(target.localPosition.z, minimumDistance, maximumDistance);
        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, clamped);
    }

    public void OnFixedUpdate()
    {
        if (isMovingObject)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);
            var targetPosition = target.position;
            var candidatePosition = currentCandidate.transform.position;
            var totalDistance = Vector3.Distance(candidatePosition, targetPosition);
            var moveTo = Vector3.ClampMagnitude(
                moveSpeed * Time.fixedDeltaTime * (targetPosition - candidatePosition).normalized,
                totalDistance
            );
            if (moveTo.magnitude > 0f)
            {
                var newPosition = candidatePosition + moveTo;
                Debug.DrawLine(targetPosition, newPosition);
                currentCandidate.MoveTowards(newPosition);
            }
            return;
        }


        lineRenderer.SetPosition(0, transform.position);
        if (!crosshair.CrosshairRaycast(out var hit))
        {
            lineRenderer.SetPosition(1, crosshair.GetInfiniteDirection());
            UnsetCurrentCandidate();
            return;
        }
        movablePoint = hit.point;
        lineRenderer.SetPosition(1, movablePoint);
        var movable = hit.transform.GetComponent<TelekinesisMovable>();
        if (movable == currentCandidate)
        {
            return;
        }
        ChangeCandidate(movable);
    }

    private void UnsetCurrentCandidate()
    {
        if (!hasCandidate)
        {
            return;
        }
        hasCandidate = false;
        currentCandidate.Unset();
        currentCandidate = null;
    }

    private void ChangeCandidate(TelekinesisMovable movable)
    {
        if (hasCandidate)
        {
            hasCandidate = false;
            currentCandidate.Unset();
            currentCandidate = null;
        }
        if (movable != null)
        {
            hasCandidate = true;
            movable.SetToCandidate();
            currentCandidate = movable;
        }
    }
}
