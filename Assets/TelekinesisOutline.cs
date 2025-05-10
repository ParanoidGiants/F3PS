using System;
using UnityEngine;

public class TelekinesisOutline : MonoBehaviour
{
    public Material hoveringOutline;
    public Material pickedOutline;

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
}
