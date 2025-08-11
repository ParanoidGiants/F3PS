using F3PS.Damage.Take;
using StarterAssets;
using UnityEngine;

public class ThrowBackAtHit : MonoBehaviour
{
    public Transform throwBackPoint;
    public float throwBackSpeed;
    public int damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<Hittable>(out var hittable))
        {
            return;
        }

        if (hittable is PlayerHittable playerHittable)
        {
            var playerController = playerHittable.owner.GetComponentInChildren<ThirdPersonController>();
            playerController.ThrowBackAt(throwBackPoint.position, throwBackSpeed);
        }

        hittable.OnHit(damage, transform.forward);
    }
}
