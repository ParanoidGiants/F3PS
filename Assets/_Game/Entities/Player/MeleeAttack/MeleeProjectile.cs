using System;
using System.Collections;
using F3PS.Damage.Take;
using UnityEngine;

public class MeleeProjectile : MonoBehaviour
{
    private Collider[] _ownerColliders;
    private HitBox _hitBox;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool collisionsEnabled = false;
    private bool _isHit = false;

    [Header("Reference")]
    public ParticleSystem hitParticleSystem;
    public ParticleSystem noHitParticleSystem;
    public GameObject[] meshes;

    [Header("Settings")]
    public int damage = 50;
    public float lifeTime = 0f;
    public float maximumLifeTimer = 5f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    private void Awake()
    {
        _hitBox = GetComponent<HitBox>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
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
        Array.ForEach(meshes, m => m.SetActive(true));
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

        Array.ForEach(meshes, m => m.SetActive(false));

        var hittable = other.gameObject.GetComponent<Hittable>();
        if (hittable != null
            && hittable.HittableId != _hitBox.attackerId
        )
        {
            hittable.OnHit(_hitBox, transform.forward);
            hitParticleSystem.gameObject.SetActive(true);
        }
        else
        {
            noHitParticleSystem.gameObject.SetActive(true);
        }
        StartCoroutine(SetInactiveAfterSeconds());
    }

    public void Init(int userSpaceId, Collider[] ownerColliders)
    {
        _hitBox.attackerId = userSpaceId;
        _ownerColliders = ownerColliders;
    }

    public void Shoot(float shootSpeed)
    {
        _isHit = false;
        collisionsEnabled = false;
        foreach (var hittableCollider in _ownerColliders)
        {
            Physics.IgnoreCollision(_collider, hittableCollider);
        }

        _rigidbody.isKinematic = false;
        _rigidbody.velocity = transform.forward * shootSpeed;
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

        yield return new WaitForSeconds(hitParticleSystem.main.duration);
        gameObject.SetActive(false);
    }
}
