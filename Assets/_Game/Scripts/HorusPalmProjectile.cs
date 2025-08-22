using F3PS;
using F3PS.Damage.Take;
using System;
using System.Collections;
using UnityEngine;

public class HorusPalmProjectile : MonoBehaviour
{
    private HorusPalmData HorusPalmData => GameManager.Instance.GameData.PlayerData.HorusPalmData;

    private Collider[] _ownerColliders;
    private HitBox _hitBox;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private PhysicsTimeObject _physicsTimeObject;
    private bool collisionsEnabled = false;
    private bool _isHit = false;

    [Header("Reference")]
    public GameObject hitParticleSystem;
    public GameObject mesh;

    [Header("Settings")]
    public GameObject owner;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;
    public float impactForceMultiplier = 1.0f;
    public float particleDuration = 1.0f;
    public float lifeTime = 0f;

    private void Awake()
    {
        _hitBox = GetComponent<HitBox>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _physicsTimeObject = GetComponent<PhysicsTimeObject>();
    }

    private void Update()
    {
        if (_isHit) return;

        lifeTime += _physicsTimeObject.ScaledDeltaTime;
        if (lifeTime > HorusPalmData.ProjectileLifeDuration)
        {
            gameObject.SetActive(false);
        }

        enableCollisionsTime += _physicsTimeObject.ScaledDeltaTime;
        if (enableCollisionsTime <= enableCollisionsTimer || collisionsEnabled)
        {
            return;
        }
        collisionsEnabled = true;
        foreach (var hittableCollider in _ownerColliders)
        {
            Physics.IgnoreCollision(_collider, hittableCollider, false);
        }
    }
    private void OnDisable()
    {
        mesh.SetActive(true);
        hitParticleSystem.SetActive(false);
    }


    public void OnCollisionEnter(Collision collision)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;

        _physicsTimeObject.DeactivateOutline();
        mesh.SetActive(false);
        hitParticleSystem.SetActive(true);
        StartCoroutine(SetInactiveAfterSeconds());

        if (collision.gameObject.TryGetComponent<Rigidbody>(out var other))
        {
            float impactForce = collision.relativeVelocity.magnitude * impactForceMultiplier;
            other.AddForce(-collision.relativeVelocity * impactForce, ForceMode.Impulse);
        }

        if (collision.gameObject.TryGetComponent<Hittable>(out var hittable)
            && !hittable.IsOwner(owner.GetInstanceID()))
        {
            Debug.Log($"HorusPalmProjectile hit {hittable.name} with damage: {HorusPalmData.Damage}");
            hittable.OnHit(HorusPalmData.Damage, transform.forward);
        }
    }

    public void Init(GameObject owner, Collider[] ownerColliders)
    {
        this.owner = owner;
        _ownerColliders = ownerColliders;
    }

    public void Shoot(float shootSpeed, float impactForce)
    {
        impactForceMultiplier = impactForce;
        _isHit = false;
        collisionsEnabled = false;
        foreach (var hittableCollider in _ownerColliders)
        {
            Physics.IgnoreCollision(_collider, hittableCollider);
        }

        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = transform.forward * shootSpeed;
        lifeTime = 0f;
        enableCollisionsTime = 0f;
        _collider.enabled = true;
    }

    private IEnumerator SetInactiveAfterSeconds()
    {
        yield return new WaitForFixedUpdate();
        _collider.enabled = false;
        yield return new WaitForFixedUpdate();
        _rigidbody.isKinematic = true;

        yield return new WaitForSeconds(particleDuration);
        gameObject.SetActive(false);
    }
}
