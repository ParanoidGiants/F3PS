using Cinemachine;
using UnityEngine;

public class HittableManager : MonoBehaviour
{
    public Collider[] colliders;
    public CinemachineImpulseSource shakeSource;
    public float shakePower;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Shake(float damageMultiplier)
    {
        shakeSource.GenerateImpulse(damageMultiplier * shakePower);
    }
}
