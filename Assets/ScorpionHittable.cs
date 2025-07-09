using F3PS.Damage.Take;
using StarterAssets;
using UnityEngine;

public class ScorpionHittable : Hittable
{
    public ScorpionController scorpion;

    void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        _collider.enabled = true;
    }

    private void OnDisable()
    {
        _collider.enabled = false;
    }

    override
    public void OnHit(int damage, Vector3 hitDirection)
    {
        if (scorpion.isDead)
        {
            return;
        }
        scorpion.HitByPlayerFrom(hitDirection);
        scorpion.Hit((int)(damageMultiplier * damage));
    }
}
