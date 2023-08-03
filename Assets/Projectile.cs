using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool Hit { get; private set; }
    public float speed = 0f;
    public int damage = 50;
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
        speed = shootSpeed;
    }
    
    private void OnEnable()
    {
        _trailRenderer.Clear();
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
    }
    
    private void OnDisable()
    {
        _rb.velocity = Vector3.zero;
    }

    private IEnumerator OnCollisionEnter(Collision other)
    {
        if (Hit) yield break;
        
        Hit = true;
        _rb.velocity *= 0.1f;
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}
