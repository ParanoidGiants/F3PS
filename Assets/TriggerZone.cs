using System;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public Action<Collider> OnTriggerZoneEnter;
    public Action<Collider> OnTriggerZoneExit;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerZoneEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerZoneExit?.Invoke(other);
    }
}
