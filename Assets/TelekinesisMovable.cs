using System;
using TMPro;
using UnityEngine;

public class TelekinesisMovable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Renderer _renderer;

    [Header("References")]
    public Material outlineMaterial;
    public Material defaultMaterial;
    public Material pickedMaterial;

    [Space(10)]
    [Header("Watcher")]
    public bool isMoving;
    public GameObject outline;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        var meshFilter = GetComponent<MeshFilter>();
        outline = new GameObject("Outline");
        outline.transform.SetParent(transform);
        outline.transform.localPosition = Vector3.zero;
        outline.transform.localRotation = Quaternion.identity;
        outline.AddComponent<MeshFilter>().mesh = meshFilter.mesh;
        outline.AddComponent<MeshRenderer>().material = outlineMaterial;
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
        _renderer.material = pickedMaterial;
    }

    private void Unpick()
    {
        _renderer.material = defaultMaterial;
    }

    public void StartMoving()
    {
        isMoving = true;
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
        Pick();
    }
    public void StopMoving()
    {
        isMoving = false;
        _rigidbody.useGravity = true;
        Unpick();
        SelectAsCandidate();
    }

    public void MoveTowards(Vector3 moveTo)
    {
        _rigidbody.MovePosition(moveTo);
    }

}
