using Cinemachine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TimeBubbleGrenadeProjectile : MonoBehaviour
{
    [Header("Reference")]
    public Transform userSpace;
    public TimeBubble timeBubble;
    public CinemachineImpulseSource shakeSource;
    public float animationDuration = 0.5f;
    public TimeBubbleHUD hud;
    private bool _isUpAndRunning = false;
    public ParticleSystem hitParticleSystem;
    public ParticleSystem noHitParticleSystem;
    public GameObject mesh;
    public HitBox hitBox;
    public Rigidbody rb;
    public Collider col;

    [Header("Settings")]
    public int damage = 50;
    public float shakePower = 1f;
    public float lifeTime = 0f;
    public float maximumLifeTimer = 5f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    public float LifeTimePercentage => lifeTime / maximumLifeTimer;
    public bool IsTimeBubbleActiveAndEnabled => timeBubble.isActiveAndEnabled;
    public bool IsProjectileUpAndRunning => _isUpAndRunning;

    private float _speed;
    private bool _isHit = false;
    private Collider[] collidersToIgnore;
    private bool _isInitialized = false;

    private void Awake()
    {
        hud = FindObjectOfType<TimeBubbleHUD>();
        hud.SetTimeScale(timeBubble.timeScale);
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
        hitParticleSystem.gameObject.SetActive(false);
        noHitParticleSystem.gameObject.SetActive(false);
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
        Debug.Log($"Touched object {_touchedTransform}");
        _stickToLocalPosition = _touchedTransform.InverseTransformPoint(transform.position);
    }

    private void Update()
    {
        if (!_isHit || !_isUpAndRunning) return;

        lifeTime += Time.deltaTime;
        hud.UpdateGrenadeEffect(LifeTimePercentage);

        transform.position = _touchedTransform.TransformPoint(_stickToLocalPosition);

        if (lifeTime > maximumLifeTimer)
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
        rb.velocity = transform.forward * _speed;
        lifeTime = 0f;
        enableCollisionsTime = 0f;
        col.enabled = true;
    }

    private void ActivateTimeBubble()
    {
        hud.SetTimeScale(timeBubble.timeScale);
        timeBubble.Clear();
        shakeSource.GenerateImpulseAt(transform.position, Vector3.one * shakePower);
        timeBubble.gameObject.SetActive(true);
        timeBubble.transform.localScale = Vector3.zero;
        timeBubble.transform
            .DOScale(Vector3.one * timeBubble.targetSize, animationDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
             {
                 _isUpAndRunning = true;
             });
    }

    public void DeactivateTimeBubble()
    {
        hud.UpdateGrenadeEffect(0f);
        _isUpAndRunning = false;
        timeBubble.gameObject.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                // Deactivate the GameObject after the tween is complete.
                timeBubble.Clear();
                timeBubble.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
    }

    public void BeforeSetActive(Vector3 position, Vector3 targetPosition, float shootSpeed)
    {
        _speed = shootSpeed;
        transform.position = position;
        transform.forward = targetPosition - position;
    }

    public void PitchTimeScale(float v)
    {
        timeBubble.PitchTimeScale(v);
        hud.SetTimeScale(timeBubble.timeScale);
    }
}
