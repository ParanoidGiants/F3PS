using F3PS.Damage.Take;
using UnityEngine;

public class RushHitBox : MonoBehaviour
{
    public GameObject owner;
    public int damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Hittable>(out var hittable)
            && !hittable.IsOwner(owner.GetInstanceID()))
        {
            hittable.OnHit(damage, (hittable.Center() - transform.position).normalized);
        }

    }
}
