using System;
using System.Collections;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Quaternion _initialRotation;

    [Header("References")]
    public TelekinesisOutline outline;

    [Space(10)]
    [Header("Watcher")]
    public bool isMoving;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var meshFilter = GetComponent<MeshFilter>();
        outline.Init(meshFilter.mesh);
        outline.SetActive(false);
    }

    public void SelectAsCandidate()
    {
        outline.SetActive(true);
    }

    public void UnselectAsCandidate()
    {
        outline.SetActive(false);
    }

    public void StartMoving()
    {
        isMoving = true;
        SetUseGravity(false);
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.velocity = Vector3.zero;
        _initialRotation = transform.rotation;
        outline.Pick();
    }
    public void StopMoving(float maximumThrowSpeed)
    {
        isMoving = false;
        SetUseGravity(true);
        outline.Unpick();
        SelectAsCandidate();
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maximumThrowSpeed);
        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    private void SetUseGravity(bool use)
    {
        var timeObject = GetComponent<PhysicsTimeObject>();
        if (timeObject != null)
        {
            timeObject.useGravity = use;
        }
        else
        {
            _rigidbody.useGravity = use;
        }
    }

    public void MoveTowards(Vector3 moveTo, float moveSpeed, Vector3 subjectRight)
    {
        Vector3 direction = (moveTo - transform.position);
        Vector3 velocity = direction * moveSpeed;
        _rigidbody.velocity = velocity;
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
        if (isInRotationCoroutine)
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

    public bool isInRotationCoroutine = false;
    private Coroutine rotateCoroutine;

    public void Rotate(RotationCommand rotationCommand, Quaternion subjectOrientation, float rotateTimer)
    {
        if (rotationCommand == RotationCommand.None)
        {
            return;
        }
        if (isInRotationCoroutine)
        {
            return;
        }
        isInRotationCoroutine = true;
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }

        rotateCoroutine = StartCoroutine(RotateCoroutine(rotationCommand, subjectOrientation, rotateTimer));
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
            Debug.Log($"Rotating: {time}s");
            time += Time.deltaTime;
            var t = Mathf.Clamp01(time / rotateTimer);
            var rotation = Quaternion.Slerp(worldStartRotation, worldTargetRotation, t);
            transform.rotation = rotation;
            yield return null;
        }
        transform.rotation = worldTargetRotation;
        isInRotationCoroutine = false;
    }
}
