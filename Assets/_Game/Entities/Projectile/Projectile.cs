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
    public bool Hit { get; private set; }

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
    }

    public void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        transform.position = position;
        transform.rotation = rotation;
        _speed = shootSpeed;
        Hit = false;
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
        
        Hit = true;
        _rb.velocity *= 0.1f;
        StartCoroutine(SetInactiveAfterSeconds(2));
    }

    private IEnumerator SetInactiveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
