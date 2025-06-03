using System.Collections;
using F3PS.Damage.Take;
using UnityEngine;

public class MeleeProjectile : MonoBehaviour
{
    private HitBox _hitBox;
    private Rigidbody _rigidbody;
    private Collider _collider;

    [Header("Reference")]
    public ParticleSystem hitParticleSystem;
    public ParticleSystem noHitParticleSystem;
    public GameObject mesh;

    [Header("Settings")]
    public int damage = 50;
    public float lifeTime = 0f;
    public float maximumLifeTimer = 5f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;
    private bool collisionsEnabled = false;

    private float _speed;

    protected bool _isHit = false;
    private Collider[] collidersToIgnore;

    private void Awake()
    {
        _hitBox = GetComponent<HitBox>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void Init(int userSpaceId, Collider[] colliders)
    {
        _hitBox.attackerId = userSpaceId;
        collidersToIgnore = colliders;
    }
    
    private void Update()
    {
        if (_isHit) return;

        lifeTime += Time.deltaTime;
        if (lifeTime > maximumLifeTimer)
        {
            gameObject.SetActive(false);
        }

        enableCollisionsTime += Time.deltaTime;
        if (enableCollisionsTime > enableCollisionsTimer && !collisionsEnabled)
        {
            collisionsEnabled = true;
            foreach (var hittableCollider in collidersToIgnore)
            {
                Physics.IgnoreCollision(_collider, hittableCollider, false);
            }
        }
    }

    public virtual void BeforeSetActive(float shootSpeed)
    {
        _speed = shootSpeed;
        _isHit = false;
        collisionsEnabled = false;
        foreach (var hittableCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(_collider, hittableCollider);
        }
    }
    
    private void OnEnable()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = transform.forward * _speed;
        lifeTime = 0f;
        enableCollisionsTime = 0f;
        _collider.enabled = true;
    }

    private void OnDisable()
    {
        mesh.SetActive(true);
        hitParticleSystem.gameObject.SetActive(false);
        noHitParticleSystem.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;

        mesh.SetActive(false);
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (hittable != null 
            && hittable.HittableId != _hitBox.attackerId
        ) {
            hittable.OnHit(_hitBox, transform.forward);
            hitParticleSystem.gameObject.SetActive(true);
        }
        else
        {
            noHitParticleSystem.gameObject.SetActive(true);
        }
        ProjectileSpecificActions();
    }

    protected virtual void ProjectileSpecificActions()
    {
        StartCoroutine(SetInactiveAfterSeconds());
    }

    private IEnumerator SetInactiveAfterSeconds()
    {
        yield return new WaitForFixedUpdate();
        _collider.enabled = false;
        yield return new WaitForFixedUpdate();
        _rigidbody.isKinematic = true;

        yield return new WaitForSeconds(hitParticleSystem.main.duration);
        gameObject.SetActive(false);
    }
}
