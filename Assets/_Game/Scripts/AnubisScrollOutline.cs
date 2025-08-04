using UnityEngine;

public class AnubisScrollOutline : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    public Material material;
    public Color picked;
    public Color recording;
    public Color resume;
    public Color rewind;
    public Color pause;
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
        _meshRenderer.material.SetColor("_Color", picked);
    }

    internal void Record()
    {
        _meshRenderer.material.SetColor("_Color", recording);
        _meshRenderer.material.SetFloat("_LineSpeed", 1.0f);
    }

    internal void Resume()
    {
        _meshRenderer.material.SetColor("_Color", resume);
        _meshRenderer.material.SetFloat("_LineSpeed", 1.0f);
    }

    internal void Rewind()
    {
        _meshRenderer.material.SetColor("_Color", rewind);
        _meshRenderer.material.SetFloat("_LineSpeed", -1.0f);
    }

    internal void Pause()
    {
        _meshRenderer.material.SetColor("_Color", pause);
        _meshRenderer.material.SetFloat("_LineSpeed", 0.0f);
    }
}
