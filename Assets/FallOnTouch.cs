using DG.Tweening;
using StarterAssets;
using UnityEngine;

public class FallOnTouch : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public Tween shakeAnimation;
    public float shakeUntilFallDuration;
    public float shakeUntilFallTime;
    public bool isTouched;
    public bool isFalling;
    private TriggerZone _triggerZone;
    private Vector3 startPosition;

    private void Awake()
    {
        _triggerZone = GetComponentInChildren<TriggerZone>();
        startPosition = transform.position;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        shakeAnimation = transform.DOShakePosition(shakeUntilFallDuration, 0.1f);
        shakeAnimation.SetAutoKill(false);
        shakeAnimation.Pause();
        _triggerZone.OnTriggerZoneEnter += OnTriggerZoneEnter;
        _triggerZone.OnTriggerZoneExit += OnTriggerZoneExit;
    }


    private void OnTriggerZoneEnter(Collider collider)
    {
        if (!collider.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }
        isTouched = true;
        shakeAnimation.Play();
    }

    private void OnTriggerZoneExit(Collider collider)
    {
        if (!collider.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }
        isTouched = false;
        shakeUntilFallTime = 0;
        shakeAnimation.Pause();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Helper.IsInLayerMask(collision.gameObject, Helper.GroundLayer))
        {
            return;
        }
        transform.position = startPosition;
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        shakeAnimation.Pause();
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
        Debug.Log(shakeUntilFallTime);
        if (shakeUntilFallTime >= shakeUntilFallDuration)
        {
            shakeAnimation.Pause();
            isFalling = true;
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
        }
    }
}
