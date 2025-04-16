using UnityEngine;

public class TelekinesisController : MonoBehaviour
{
    [Header("References")]
    public Crosshair crosshair;
    public LayerMask movableLayer;
    public LineRenderer lineRenderer;
    public TelekinesisMovable currentCandidate;
    public Transform target;

    [Space(10)]
    [Header("Settings")]
    public float pushPullSpeed = 5.0f;

    [Space(10)]
    [Header("Watchers")]
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
            target.position = currentCandidate.transform.position;
            currentCandidate.StartMoving();
            isMovingObject = true;
        }
        else if (isMovingObject && !isShooting)
        {
            currentCandidate.StopMoving();
            isMovingObject = false;
        }
        else
        {
            HandlePushPull(pushPull);
        }

    }

    private void HandlePushPull(float pushPull)
    {
        if (pushPull == 0f)
        {
            return;
        }

        var moveDirection = pushPull * target.forward;
        var moveTarget = target.position + moveDirection * Time.deltaTime * pushPullSpeed;
        target.position = moveTarget;
    }
    public void OnFixedUpdate()
    {
        if (isMovingObject)
        {
            currentCandidate.OnFixedUpdate(target);
            return;
        }
        lineRenderer.SetPosition(0, transform.position);
        if (!crosshair.CrosshairRaycast(out var hit))
        {
            lineRenderer.SetPosition(1, crosshair.GetInfiniteDirection());
            UnsetCurrentCandidate();
            return;
        }
        lineRenderer.SetPosition(1, hit.point);
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
