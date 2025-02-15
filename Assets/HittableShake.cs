using Cinemachine;
using UnityEngine;

public class HittableShake : MonoBehaviour
{
    [Header("References")]
    public CinemachineImpulseSource shakeSource;

    [Space(10)]
    [Header("Shake Settings")]
    public float shakePower;

    public void Shake(float damageMultiplier)
    {
        shakeSource.GenerateImpulse(damageMultiplier * shakePower);
    }
}
