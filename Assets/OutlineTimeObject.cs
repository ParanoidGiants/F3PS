using UnityEngine;

public class OutlineTimeObject : MonoBehaviour
{
    private bool initialized = false;
    private MeshRenderer _meshRenderer;
    public MeshFilter meshFilterToCloneFrom;
    public Material material;
    public Color freeze;
    public Color normal;
    private void OnEnable()
    {
        if (initialized) return;

        _meshRenderer = GetComponent<MeshRenderer>();
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = meshFilterToCloneFrom.mesh;
        initialized = true;
    }

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
}
