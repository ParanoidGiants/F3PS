using UnityEngine;

public class OutlineTimeObject : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    public MeshFilter meshFilterToCloneFrom;
    public Material material;
    public Color freeze;
    public Color normal;

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Pitch(float timeScale)
    {
        var color = Color.Lerp(freeze, normal, timeScale);
        _meshRenderer.material.SetColor("_Color", color);
        _meshRenderer.material.SetFloat("_LineSpeed", 2 * timeScale);
    }

    internal void Init()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = meshFilterToCloneFrom.mesh;
    }
}
