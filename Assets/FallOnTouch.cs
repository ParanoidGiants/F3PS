using DG.Tweening;
using StarterAssets;
using UnityEngine;

public class FallOnTouch : MonoBehaviour
{
    private RigidbodyHub _rigidbodyHub;
    public Tween shakeAnimation;
    public float shakeUntilFallDuration;
    public float shakeUntilFallTime;
    public bool isTouched;
    public bool isFalling;
    private TriggerZone _triggerZone;
    private Vector3 startPosition;
    private Quaternion startRotation;


    private void Awake()
    {
        _triggerZone = GetComponentInChildren<TriggerZone>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        _rigidbodyHub = GetComponent<RigidbodyHub>();
        _rigidbodyHub.SetKinematic();
        _rigidbodyHub.useGravity = false;
        if (shakeUntilFallDuration != 0f)
        {
            shakeAnimation = transform.DOShakePosition(shakeUntilFallDuration, 0.1f);
            shakeAnimation.SetAutoKill(false);
            shakeAnimation.Pause();
        }
        _triggerZone.OnTriggerZoneEnter += OnTriggerZoneEnter;
        _triggerZone.OnTriggerZoneExit += OnTriggerZoneExit;
    }


    private void OnTriggerZoneEnter(Collider collider)
    {
        if (!collider.TryGetComponent<ThirdPersonController>(out var _))
        {
            return;
        }
        isTouched = true;
        shakeAnimation?.Play();
    }

    private void OnTriggerZoneExit(Collider collider)
    {
        if (!collider.TryGetComponent<ThirdPersonController>(out var _))
        {
            return;
        }
        isTouched = false;
        shakeUntilFallTime = 0;
        shakeAnimation?.Pause();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Helper.IsInLayerMask(collision.gameObject, Helper.GroundLayer))
        {
            return;
        }
        transform.position = startPosition;
        transform.rotation = startRotation;
        _rigidbodyHub.useGravity = false;
        _rigidbodyHub.SetKinematic();
        shakeAnimation?.Pause();
        isFalling = false;
        isTouched = false;
    }

    private void Update()
    {
        if (!isTouched || isFalling)
        {
            return;
        }

        shakeUntilFallTime += Time.deltaTime;
        if (shakeUntilFallTime >= shakeUntilFallDuration)
        {
            shakeAnimation?.Pause();
            isFalling = true;
            _rigidbodyHub.useGravity = true;
            _rigidbodyHub.UnsetKinematic();
        }
    }
}
