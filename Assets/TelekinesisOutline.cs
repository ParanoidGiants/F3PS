using System;
using UnityEngine;

public class TelekinesisOutline : MonoBehaviour
{
    public Material hoveringOutline;
    public Material pickedOutline;
    public Material rotateOutline;

    private MeshRenderer _meshRenderer;

    public void Init(Mesh mesh)
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    internal void Pick()
    {
        _meshRenderer.material = pickedOutline;
    }

    internal void Unpick()
    {
        _meshRenderer.material = hoveringOutline;
    }

    public void StartRotate()
    {
        _meshRenderer.material = rotateOutline;
    }

    public void StopRotate()
    {
        _meshRenderer.material = pickedOutline;
    }
}
