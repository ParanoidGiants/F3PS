using System;
using System.Collections;
using F3PS;
using F3PS.Damage.Take;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class YaghotepSpawnProjectile : MonoBehaviour
{
    private Collider[] _ownerColliders;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private PhysicsTimeObject _physicsTimeObject;
    private bool collisionsEnabled = false;
    private bool _isHit = false;

    [Header("Reference")]
    public GameObject hitParticleSystem;
    public GameObject mesh;
    public GameObject spawnYaggiStandardPrefab;
    public GameObject spawnYaggiSpitterPrefab;
    public GameObject spawnYaggiShieldPrefab;

    [Header("Settings")]
    public GameObject owner;
    public float projectileLifeDuration = 5f;
    public int damage = 1;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;

    [Header("Watchers")]
    public float lifeTime = 0f;
    public Action<GameObject> onEnemySpawned;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _physicsTimeObject = GetComponent<PhysicsTimeObject>();
    }

    private void Update()
    {
        if (_isHit) return;

        lifeTime += _physicsTimeObject.ScaledDeltaTime;
        if (lifeTime > projectileLifeDuration)
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
        hitParticleSystem.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
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

        var random = UnityEngine.Random.Range(0, 3);
        GameObject enemyPrefab = null;
        if (random == 0)
        {
            enemyPrefab = spawnYaggiStandardPrefab;
        }
        else if (random == 1)
        {
            enemyPrefab = spawnYaggiSpitterPrefab;
        }
        else
        {
            enemyPrefab = spawnYaggiShieldPrefab;
        }

        Vector3 intendedSpawn = transform.position;
        NavMeshHit hit;
        var forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        if (NavMesh.SamplePosition(intendedSpawn, out hit, 20f, NavMesh.AllAreas))
        {
            var enemy = Instantiate(enemyPrefab, hit.position, rotation);
            onEnemySpawned?.Invoke(enemy);
        }
        else
        {
            Debug.LogError("No NavMesh at intended spawn position!");
        }

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

    public void Shoot(float shootSpeed, float gravityScale, Action<GameObject> onEnemySpawnedCallback)
    {
        _isHit = false;
        onEnemySpawned = onEnemySpawnedCallback;
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
