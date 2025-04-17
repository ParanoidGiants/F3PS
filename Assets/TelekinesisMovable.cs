using System;
using TMPro;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    public Rigidbody rigidbody_;
    public Renderer renderer_;
    public Material unset;
    public Material candidate;
    public Material selected;

    public bool isMoving;

    public void SetToCandidate()
    {
        renderer_.material = candidate;
    }
    public void Unset()
    {
        renderer_.material = unset;
    }
    public void Select()
    {
        renderer_.material = selected;
    }
    public void StartMoving()
    {
        isMoving = true;
        rigidbody_.useGravity = false;
        rigidbody_.velocity = Vector3.zero;
        Select();
    }
    public void StopMoving()
    {
        isMoving = false;
        rigidbody_.useGravity = true;
        SetToCandidate();
    }

    public void MoveTowards(Vector3 moveTo)
    {
        rigidbody_.MovePosition(moveTo);
    }

}
