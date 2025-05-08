using System;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Renderer _renderer;

    [Header("References")]
    public TelekinesisOutline outline;

    [Space(10)]
    [Header("Watcher")]
    public bool isMoving;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        var meshFilter = GetComponent<MeshFilter>();
        outline.Init(meshFilter.mesh);
        outline.SetActive(false);
    }

    public void SelectAsCandidate()
    {
        outline.SetActive(true);
    }

    public void UnselectAsCandidate()
    {
        outline.SetActive(false);
    }

    public void Pick()
    {
        outline.Pick();
    }

    private void Unpick()
    {
        outline.Unpick();
    }

    public void StartMoving()
    {
        isMoving = true;
        SetUseGravity(false);
        _rigidbody.velocity = Vector3.zero;
        Pick();
    }
    public void StopMoving()
    {
        isMoving = false;
        SetUseGravity(true);
        Unpick();
        SelectAsCandidate();
    }

    private void SetUseGravity(bool use)
    {
        var timeObject = GetComponent<PhysicsTimeObject>();
        if (timeObject != null)
        {
            timeObject.useGravity = use;
        }
        else
        {
            _rigidbody.useGravity = use;
        }
    }

    public void MoveTowards(Vector3 moveTo, float moveSpeed)
    {
        Vector3 direction = (moveTo - transform.position);
        Vector3 velocity = direction * moveSpeed;
        _rigidbody.velocity = velocity;
    }

}
