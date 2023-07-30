using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _speed = 0f;
    private Rigidbody _rb;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    public void BeforeSetActive(Vector3 position, Quaternion rotation, float shootSpeed)
    {
        transform.position = position;
        transform.rotation = rotation;
        _speed = shootSpeed;
    }
    
    private void OnEnable()
    {
        _trailRenderer.Clear();
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * _speed;
    }
    
    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }

    private IEnumerator OnCollisionEnter(Collision other)
    {
        _rb.velocity *= 0.1f;
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}
