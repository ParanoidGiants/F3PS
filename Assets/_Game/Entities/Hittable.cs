using UnityEngine;

public class Hittable : MonoBehaviour
{
    private Collider _collider;
    
    void Awake()
    {
        _collider = GetComponent<Collider>();

    }
    
    public Vector3 Center()
    {
        return _collider.bounds.center;
    }
}
