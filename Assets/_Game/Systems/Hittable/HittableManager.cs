using UnityEngine;

public class HittableManager : MonoBehaviour
{
    [Header("Watchers")]
    public Collider[] colliders;


    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }
}
