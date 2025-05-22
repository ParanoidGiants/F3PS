using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityLine : MonoBehaviour
{
    public Rigidbody rb;
    public LineRenderer velocityLine;

    private void Awake()
    {
        velocityLine.positionCount = 2;
        velocityLine.SetPosition(0, transform.position);
        velocityLine.SetPosition(1, transform.position);
    }

    private void Update()
    {
        velocityLine.SetPosition(0, transform.position);
        velocityLine.SetPosition(1, transform.position + rb.velocity);
    }
}
