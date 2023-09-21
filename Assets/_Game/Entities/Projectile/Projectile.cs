using System;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 50;
    public float lifeTime = 0f;
    public float maximumLifeTime = 5f;
    
    private HitBox _hitBox;
    private Rigidbody _rb;
    private TrailRenderer _trailRenderer;
    private float _speed;
    [SerializeField] private bool _isHit = false;
    public bool Hit => _isHit;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _hitBox = GetComponent<HitBox>();
        _rb = GetComponent<Rigidbody>();
    }

    public void Init(int attackerId)
    {
        _hitBox.attackerId = attackerId;
    }
    
    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > maximumLifeTime)
        {
            gameObject.SetActive(false);
        }

        if (!Hit)
        {
            transform.forward = _rb.velocity;
        }
    }

    public void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        transform.position = position;
        transform.rotation = rotation;
        _speed = shootSpeed;
        _isHit = false;
    }
    
    private void OnEnable()
    {
        _trailRenderer.Clear();
        _rb.velocity = transform.forward * _speed;
        lifeTime = 0f;
    }

    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }

    public void SetHit()
    {
        if (Hit) return;
        
        _isHit = true;
        _rb.velocity *= 0.1f;
        StartCoroutine(SetInactiveAfterSeconds(2));
    }

    private IEnumerator SetInactiveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
