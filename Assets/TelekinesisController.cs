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
    public Vector3 contactPoint;
    public bool isMovingObject = false;
    public bool hasCandidate = false;

    public void OnDisable()
    {
        target.gameObject.SetActive(false);
    }

    public void OnUpdate(bool isShooting, float pushPull)
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, contactPoint);
        if (!hasCandidate)
        {
            return;
        }

        if (!isMovingObject &&  isShooting)
        {
            target.gameObject.SetActive(true);
            SetTargetPosition(contactPoint);
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
            var moveDirection = target.forward;
            var moveTarget = target.position + pushPull * pushPullSpeed * Time.deltaTime * moveDirection;

            SetTargetPosition(moveTarget);
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

    public void OnFixedUpdate()
    {
        if (isMovingObject)
        {
            contactPoint = target.position;
            currentCandidate.MoveTowards(target.position, moveSpeed);
            return;
        }
        if (!crosshair.CrosshairRaycast(out var hit))
        {
            contactPoint = crosshair.GetInfiniteDirection();

            if (!hasCandidate)
            {
                return;
            }
            hasCandidate = false;
            currentCandidate.UnselectAsCandidate();
            currentCandidate = null;
            return;
        }
        contactPoint = hit.point;
        var movable = hit.transform.GetComponent<TelekinesisMovable>();
        if (movable == currentCandidate)
        {
            return;
        }

        if (hasCandidate)
        {
            hasCandidate = false;
            currentCandidate.UnselectAsCandidate();
            currentCandidate = null;
        }
        if (movable != null)
        {
            hasCandidate = true;
            movable.SelectAsCandidate();
            currentCandidate = movable;
        }
    }
}
