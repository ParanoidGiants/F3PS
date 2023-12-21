using System.Collections;
using F3PS.Damage.Take;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 50;
    public float lifeTime = 0f;
    public float maximumLifeTime = 5f;
    public float removeAfterSeconds = .2f;
    
    private float _speed;
    private HitBox _hitBox;
    protected Rigidbody _rb;
    protected ProjectileTimeObject _timeObject;
    protected bool _isHit = false;
    
    [Header("Watchers")]
    public bool isPlayer;
    
    public bool Hit => _isHit;

    private void Awake()
    {
        InitReferences();
    }

    public virtual void InitReferences()
    {
        _hitBox = GetComponent<HitBox>();
        _rb = GetComponent<Rigidbody>();
        _timeObject = GetComponent<ProjectileTimeObject>();
    }

    public void Init(int attackerId, bool isPlayer = false)
    {
        this.isPlayer = isPlayer;
        _hitBox.attackerId = attackerId;
    }
    
    private void Update()
    {
        lifeTime += _timeObject.ScaledDeltaTime;
        if (lifeTime > maximumLifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        transform.position = position;
        transform.rotation = rotation;
        _speed = shootSpeed;
        _isHit = false;
    }
    
    private void OnEnable()
    {
        _timeObject.ClearTrail();
        _rb.velocity = transform.forward * _speed;
        lifeTime = 0f;
    }

    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }

    public virtual void SetHit()
    {
        if (Hit) return;
        
        _isHit = true;
        StartCoroutine(SetInactiveAfterSeconds(removeAfterSeconds));
    }

    protected IEnumerator SetInactiveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        var hittable = other.gameObject.GetComponent<Hittable>();
        Debug.Log("HIT: " + other.transform.name);
        if (hittable != null 
            && hittable.hittableId != _hitBox.attackerId
        ) {
            hittable.OnHit(_hitBox);
            if (isPlayer && hittable is EnemyHittable enemyHittable)
            {
                enemyHittable.OnHitByPlayer((-1) * other.impulse);
            }
        }
        SetHit();
    }
}
