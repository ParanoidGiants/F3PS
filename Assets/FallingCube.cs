using StarterAssets;
using System;
using UnityEngine;

public class FallingCube : MonoBehaviour
{
    public bool isTouchedByPlayer = false;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        GetComponent<Rigidbody>().useGravity = false;
    }
    internal void Reset()
    {
        isTouchedByPlayer = false;
        GetComponent<Rigidbody>().useGravity = false;
        transform.position = startPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isTouchedByPlayer)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            isTouchedByPlayer = true;
        }
    }
}
