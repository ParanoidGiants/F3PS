using StarterAssets;
using UnityEngine;

public class ThrowBackAtHit : MonoBehaviour
{
    public Transform throwBackPoint;
    public float throwBackSpeed;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<ThirdPersonController>(out var player))
        {
            return;
        }

        player.ThrowBackAt(throwBackPoint.position, throwBackSpeed);
    }
}
