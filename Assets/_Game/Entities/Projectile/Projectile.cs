using System;
using System.Collections;
using Enemy;
using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool _hit;
    public float speed = 0f;
    public int damage = 50;
    private Rigidbody _rb;
    private TrailRenderer _trailRenderer;
    public float lifeTime = 0f;
    public float maximumLifeTime = 5f;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
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
        speed = shootSpeed;
        _hit = false;
    }
    
    private void OnEnable()
    {
        _trailRenderer.Clear();
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
        lifeTime = 0f;
    }

    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_hit) return;
        
        _hit = true;
        _rb.velocity *= 0.1f;
        StartCoroutine(SetInactiveAfterSeconds(2));
        
        if (Helper.IsLayerEnemyLayer(other.gameObject.layer))
        {
            var enemy = other.gameObject.GetComponent<BaseEnemy>();
            enemy.Hit(damage);
        }
        
        if (Helper.IsLayerPlayerLayer(other.gameObject.layer))
        {
            var player = other.gameObject.GetComponentInParent<ThirdPersonController>();
            player.Hit(damage);
        }
    }

    private IEnumerator SetInactiveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
