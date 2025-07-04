using System.Collections;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    private RigidbodyHub _rigidbodyHub;
    private Quaternion _initialRotation;

    [Header("References")]
    public TelekinesisOutline outline;

    [Space(10)]
    [Header("Watcher")]
    public bool isMoving;
    public bool isBeingRotated = false;
    public bool isCurrentlyTouchedByPlayer = false;
    private Coroutine _rotateCoroutine;
    public bool IsLocked => _rigidbodyHub.isAnubisScrolling;

    private void Awake()
    {
        _rigidbodyHub = GetComponent<RigidbodyHub>();
        var meshFilter = GetComponent<MeshFilter>();
        outline.Init(meshFilter.mesh);
        outline.gameObject.SetActive(false);
    }

    public void SetLocked()
    {
        outline.Lock();
    }
    public void SetUnpicked()
    {
        outline.Unpick();
    }

    public void SelectAsCandidate()
    {
        outline.gameObject.SetActive(true);
    }

    public void UnselectAsCandidate()
    {
        outline.gameObject.SetActive(false);
    }

    public void StartMoving()
    {
        isMoving = true;
        _rigidbodyHub.StartTelekinesisMoving();
        _initialRotation = transform.rotation;
        outline.Pick();
    }
    public void StopMoving(float maximumThrowSpeed)
    {
        isMoving = false;
        _rigidbodyHub.StopTelekinesisMoving(maximumThrowSpeed);
        outline.Unpick();
    }

    public void MoveTowards(Vector3 moveTo, float moveSpeed, Vector3 subjectRight)
    {
        Vector3 direction = (moveTo - transform.position);
        Vector3 velocity = direction * moveSpeed;
        _rigidbodyHub.SetTelekinesisVelocity(velocity);
    }

    public void StartRotating()
    {
        outline.StartRotate();
    }

    public void StopRotating()
    {
        outline.StopRotate();
    }

    public void SnapToRelativeRotation(Quaternion subjectOrientation)
    {
        if (isBeingRotated)
        {
            return;
        }

        var worldRotation = transform.rotation;
        var objectRotation = Quaternion.Inverse(subjectOrientation) * worldRotation;
        var snappedObjectRotation = TelekinesisController.GetClosestRotation(objectRotation);
        var snappedWorldRotation = subjectOrientation * snappedObjectRotation;
        transform.rotation = snappedWorldRotation;
    }

    public void UpdateOrientation(Quaternion subjectOrientation)
    {
        transform.rotation = subjectOrientation * _initialRotation;
    }

    public void Rotate(RotationCommand rotationCommand, Quaternion subjectOrientation, float rotateTimer)
    {
        if (rotationCommand == RotationCommand.None)
        {
            return;
        }
        if (isBeingRotated)
        {
            return;
        }
        isBeingRotated = true;
        if (_rotateCoroutine != null)
        {
            StopCoroutine(_rotateCoroutine);
        }

        _rotateCoroutine = StartCoroutine(RotateCoroutine(rotationCommand, subjectOrientation, rotateTimer));
    }

    private IEnumerator RotateCoroutine(RotationCommand rotationCommand, Quaternion subjectOrientation, float rotateTimer)
    {
        var worldStartRotation = transform.rotation;
        var objectStartRotation = Quaternion.Inverse(subjectOrientation) * worldStartRotation;
        var rotationToApply = TelekinesisController.GetStepRotationByRotationCommand(rotationCommand);
        var objectTargetRotation = TelekinesisController.GetClosestRotation(rotationToApply * objectStartRotation);
        var worldTargetRotation = subjectOrientation * objectTargetRotation;

        var time = 0f;
        while (time < rotateTimer)
        {
            time += Time.deltaTime;
            var t = Mathf.Clamp01(time / rotateTimer);
            var rotation = Quaternion.Slerp(worldStartRotation, worldTargetRotation, t);
            transform.rotation = rotation;
            yield return null;
        }
        transform.rotation = worldTargetRotation;
        isBeingRotated = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isCurrentlyTouchedByPlayer = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isCurrentlyTouchedByPlayer = false;
        }
    }
}
