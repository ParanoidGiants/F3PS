using Unity.Cinemachine;
using DG.Tweening;
using UnityEngine;
using F3PS;

public class KhonsuSphereProjectile : MonoBehaviour
{
    private PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;
    private KhonsuSphereSkillData KhonsuSphereData => PlayerEventController.Data.KhonsuSphereSkillData;

    [Header("Debug")]
    public Transform _touchedTransform;
    public Vector3 _stickToLocalPosition;
    public Rigidbody _rigidbody;
    public bool _isInitialized = false;
    public bool _isHit = false;
    public bool _isUpAndRunning = false;
    public float _speed;
    public Collider[] _collidersToIgnore;

    [Header("Reference")]
    public Transform userSpace;
    public KhonsuSphere khonsuSphere;
    public CinemachineImpulseSource shakeSource;
    public float animationDuration = 0.5f;
    public GameObject mesh;
    public HitBox hitBox;
    public Collider _collider;

    [Header("Settings")]
    public float shakePower = 1f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    public bool IsKhonsuSphereActiveAndEnabled => khonsuSphere.isActiveAndEnabled;
    public bool IsProjectileUpAndRunning => _isUpAndRunning;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

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

        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        transform.SetParent(userSpace);
        khonsuSphere.gameObject.SetActive(false);
        SetupProjectile();
    }

    private void OnDisable()
    {
        mesh.SetActive(true);

    }

    private void OnCollisionEnter(Collision other)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;
        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        ActivateKhonsuSphere();
        _touchedTransform = other.transform;
        _stickToLocalPosition = _touchedTransform.InverseTransformPoint(other.GetContact(0).point);
    }

    private void Update()
    {
        if (!_isHit || !_isUpAndRunning) return;

        var activeTime = KhonsuSphereData.ActiveTime + Time.deltaTime;
        PlayerEventController.SetKhonsuSphereActiveTime(activeTime);

        if (activeTime > KhonsuSphereData.ActiveDuration)
        {
            DeactivateKhonsuSphere();
        }
    }

    private void FixedUpdate()
    {
        if (!_isHit && _touchedTransform == null) return;

        _rigidbody.MovePosition(_touchedTransform.TransformPoint(_stickToLocalPosition));
    }

    public void Init(Collider[] colliders)
    {
        _collidersToIgnore = colliders;
        _isInitialized = true;
    }

    private void SetupProjectile()
    {
        _isHit = false;
        foreach (var hittableCollider in _collidersToIgnore)
        {
            Physics.IgnoreCollision(_collider, hittableCollider);
        }
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = transform.forward * _speed;
        PlayerEventController.SetKhonsuSphereActiveTime(0f);
        enableCollisionsTime = 0f;
        _collider.enabled = true;
    }

    private void ActivateKhonsuSphere()
    {
        khonsuSphere.Clear();
        shakeSource.GenerateImpulseAt(transform.position, Vector3.one * shakePower);
        khonsuSphere.gameObject.SetActive(true);
        khonsuSphere.transform.localScale = Vector3.zero;
        khonsuSphere.transform
            .DOScale(Vector3.one * KhonsuSphereData.TargetSize, animationDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
             {
                 _isUpAndRunning = true;
             });
    }

    public void DeactivateKhonsuSphere()
    {
        PlayerEventController.SetKhonsuSphereActiveTime(0f);
        _isUpAndRunning = false;
        khonsuSphere.gameObject.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                khonsuSphere.Clear();
                khonsuSphere.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
    }

    public void InterruptThrow()
    {
        _isUpAndRunning = false;
        khonsuSphere.Clear();
        khonsuSphere.gameObject.SetActive(false);
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
        var timeScale = KhonsuSphereData.TimeScale + changeDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);
        PlayerEventController.SetKhonsuSphereTimeScale(timeScale);
    }
}
