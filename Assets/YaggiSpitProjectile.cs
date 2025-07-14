using System;
using System.Collections;
using F3PS;
using F3PS.Damage.Take;
using UnityEngine;

public class YaggiSpitProjectile : MonoBehaviour
{
    private Collider[] _ownerColliders;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool collisionsEnabled = false;
    private bool _isHit = false;

    [Header("Reference")]
    public GameObject hitParticleSystem;
    public GameObject mesh;

    [Header("Settings")]
    public GameObject owner;
    public float projectileLifeDuration = 5f;
    public int damage = 1;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    [Header("Watchers")]
    public float lifeTime = 0f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_isHit) return;

        lifeTime += Time.deltaTime;
        if (lifeTime > projectileLifeDuration)
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
        mesh.SetActive(true);
        hitParticleSystem.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;

        mesh.SetActive(false);
        hitParticleSystem.SetActive(true);
        StartCoroutine(SetInactiveAfterSeconds());

        if (collision.gameObject.TryGetComponent<Hittable>(out var hittable)
            && !hittable.IsOwner(owner.GetInstanceID()))
        {
            hittable.OnHit(damage, transform.forward);
        }
    }

    public void Init(GameObject owner, Collider[] ownerColliders)
    {
        this.owner = owner;
        _ownerColliders = ownerColliders;
    }

    public void Shoot(float shootSpeed, float gravityScale)
    {
        _isHit = false;
        collisionsEnabled = false;
        foreach (var hittableCollider in _ownerColliders)
        {
            Physics.IgnoreCollision(_collider, hittableCollider);
        }
        GetComponent<RigidbodyHub>().gravityScale = gravityScale;
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

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}
