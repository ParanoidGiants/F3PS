using System;
using TMPro;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Renderer renderer;
    public Material unset;
    public Material candidate;
    public Material selected;

    public bool isMoving;
    public float pushPull;
    public float speed = 10f;

    public void SetToCandidate()
    {
        renderer.material = candidate;
    }
    public void Unset()
    {
        renderer.material = unset;
    }
    public void Select()
    {
        renderer.material = selected;
    }
    public void StartMoving()
    {
        isMoving = true;
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.zero;
        Select();
    }
    public void StopMoving()
    {
        isMoving = false;
        rigidbody.useGravity = true;
        pushPull = 0f;
        SetToCandidate();
    }
    public void OnFixedUpdate(Transform target)
    {
        var moveDirection = (target.position - transform.position).normalized;
        if (Vector3.Distance(transform.position, target.position) < moveDirection.magnitude)
        {
            return;
        }
        else
        {
            var moveTarget = transform.position + moveDirection * speed * Time.fixedDeltaTime;
            rigidbody.MovePosition(moveTarget);
        }
    }

}
