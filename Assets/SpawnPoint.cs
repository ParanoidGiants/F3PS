using System;
using StarterAssets;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int index;
    public Action<int> OnSpawnPointEntered;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<ThirdPersonController>(out _))
        {
            return;
        }
        OnSpawnPointEntered?.Invoke(index);
    }
}
