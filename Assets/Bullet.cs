using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        _rb.velocity *= 0.1f;
        Destroy(gameObject, 1f);
    }
}
