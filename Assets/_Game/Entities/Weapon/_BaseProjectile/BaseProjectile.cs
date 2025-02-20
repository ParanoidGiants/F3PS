using System.Collections;
using F3PS.Damage.Take;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [Header("Reference")]
    public ParticleSystem hitParticleSystem;
    public ParticleSystem noHitParticleSystem;
    public GameObject mesh;
    public ProjectileTimeObject timeObject;
    public HitBox hitBox;
    public Rigidbody rb;
    public Collider col;

    [Header("Settings")]
    public int damage = 50;
    public float lifeTime = 0f;
    public float maximumLifeTimer = 5f;
    public float enableCollisionsTime = 0f;
    public float enableCollisionsTimer = .2f;
    private bool collisionsEnabled = false;

    private float _speed;

    protected bool _isHit = false;
    private HittableManager _hittableManager;

    public void Init(int userSpaceId, HittableManager hittableManager)
    {
        hitBox.attackerId = userSpaceId;
        _hittableManager = hittableManager;
    }
    
    private void Update()
    {
        if (_isHit) return;

        lifeTime += timeObject.ScaledDeltaTime;
        if (lifeTime > maximumLifeTimer)
        {
            gameObject.SetActive(false);
        }

        enableCollisionsTime += Time.deltaTime;
        if (enableCollisionsTime > enableCollisionsTimer && !collisionsEnabled)
        {
            collisionsEnabled = true;
            foreach (var hittableCollider in _hittableManager.colliders)
            {
                Physics.IgnoreCollision(col, hittableCollider, false);
            }
        }
    }

    public virtual void BeforeSetActive(Vector3 position, Vector3 targetPosition, float shootSpeed)
    {
        transform.position = position;
        transform.forward = targetPosition - position;
        _speed = shootSpeed;
        _isHit = false;
        collisionsEnabled = false;
        foreach (var hittableCollider in _hittableManager.colliders)
        {
            Physics.IgnoreCollision(col, hittableCollider);
        }
    }
    
    private void OnEnable()
    {
        timeObject.ClearTrail();
        rb.isKinematic = false;
        rb.velocity = transform.forward * _speed;
        lifeTime = 0f;
        enableCollisionsTime = 0f;
    }

    private void OnDisable()
    {
        mesh.SetActive(true);
        hitParticleSystem.gameObject.SetActive(false);
        noHitParticleSystem.gameObject.SetActive(false);
    }

    private IEnumerator SetInactiveAfterSeconds()
    {
        yield return new WaitForSeconds(hitParticleSystem.main.duration);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_isHit)
        {
            return;
        }
        _isHit = true;

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        timeObject.ClearTrail();
        mesh.SetActive(false);
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (hittable != null 
            && hittable.HittableId != hitBox.attackerId
        ) {
            hittable.OnHit(hitBox, transform.forward);
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
}
