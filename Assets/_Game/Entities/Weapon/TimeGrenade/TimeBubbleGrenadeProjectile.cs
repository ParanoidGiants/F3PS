using Unity.Cinemachine;
using DG.Tweening;
using UnityEngine;
using F3PS;

public class TimeBubbleGrenadeProjectile : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.PlayerEventController;
    private TimeBubbleSkillData TimeBubbleData => PlayerEventController.Data.TimeBubbleSkillData;

    [Header("Reference")]
    public Transform userSpace;
    public TimeBubble timeBubble;
    public CinemachineImpulseSource shakeSource;
    public float animationDuration = 0.5f;
    private bool _isUpAndRunning = false;
    public GameObject mesh;
    public HitBox hitBox;
    public Rigidbody rb;
    public Collider col;

    [Header("Settings")]
    public float shakePower = 1f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    public bool IsTimeBubbleActiveAndEnabled => timeBubble.isActiveAndEnabled;
    public bool IsProjectileUpAndRunning => _isUpAndRunning;

    private float _speed;
    private bool _isHit = false;
    private Collider[] collidersToIgnore;
    private bool _isInitialized = false;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!_isInitialized)
        {
            return;
        }

        transform.SetParent(userSpace);
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        col.enabled = true;
        timeBubble.gameObject.SetActive(false);
        SetupProjectile();
    }

    private void OnDisable()
    {
        mesh.SetActive(true);
    }

    Transform _touchedTransform;
    Vector3 _stickToLocalPosition;

    private void OnCollisionEnter(Collision other)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        col.enabled = false;
        ActivateTimeBubble();
        _touchedTransform = other.transform;
        _stickToLocalPosition = _touchedTransform.InverseTransformPoint(transform.position);
    }

    private void Update()
    {
        if (!_isHit || !_isUpAndRunning) return;

        var activeTime = TimeBubbleData.ActiveTime + Time.deltaTime;
        PlayerEventController.SetTimeBubbleActiveTime(activeTime);

        transform.position = _touchedTransform.TransformPoint(_stickToLocalPosition);

        if (activeTime > TimeBubbleData.ActiveDuration)
        {
            DeactivateTimeBubble();
        }
    }

    public void Init(Collider[] colliders)
    {
        collidersToIgnore = colliders;
        _isInitialized = true;
    }

    private void SetupProjectile()
    {
        _isHit = false;
        foreach (var hittableCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(col, hittableCollider);
        }
        rb.isKinematic = false;
        rb.linearVelocity = transform.forward * _speed;
        PlayerEventController.SetTimeBubbleActiveTime(0f);
        enableCollisionsTime = 0f;
        col.enabled = true;
    }

    private void ActivateTimeBubble()
    {
        timeBubble.Clear();
        shakeSource.GenerateImpulseAt(transform.position, Vector3.one * shakePower);
        timeBubble.gameObject.SetActive(true);
        timeBubble.transform.localScale = Vector3.zero;
        timeBubble.transform
            .DOScale(Vector3.one * TimeBubbleData.TargetSize, animationDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
             {
                 _isUpAndRunning = true;
             });
    }

    public void DeactivateTimeBubble()
    {
        PlayerEventController.SetTimeBubbleActiveTime(0f);
        _isUpAndRunning = false;
        timeBubble.gameObject.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                timeBubble.Clear();
                timeBubble.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
    }

    public void InterruptThrow()
    {
        _isUpAndRunning = false;
        timeBubble.Clear();
        timeBubble.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void BeforeSetActive(Vector3 position, Vector3 targetPosition, float shootSpeed)
    {
        _speed = shootSpeed;
        transform.position = position;
        transform.forward = targetPosition - position;
    }

    public void PitchTimeScale(float changeDirection)
    {
        var timeScale = TimeBubbleData.TimeScale + changeDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);
        PlayerEventController.SetTimeBubbleTimeScale(timeScale);
    }
}
