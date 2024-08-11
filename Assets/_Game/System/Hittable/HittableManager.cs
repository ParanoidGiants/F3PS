using UnityEngine;

public class HittableManager : MonoBehaviour
{
    public Collider[] colliders;
    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }
}
