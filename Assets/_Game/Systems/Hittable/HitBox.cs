using F3PS.Damage.Take;
using System;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int attackerId;
    public int damage;

    public void OnTriggerEnter(Collider other)
    {
        var hittable = other.GetComponent<Hittable>();
        if (hittable == null || hittable.HittableId == attackerId)
        {
            return;
        }
        hittable.OnHit(this, transform.forward);
    }
}
