using UnityEngine;

public class YaggiSpitterHittable : Hittable
{
    public YaggiSpitterController yaggi;

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
        if (yaggi.isDead)
        {
            return;
        }
        yaggi.HitByPlayerFrom(hitDirection);
        yaggi.Hit((int)(damageMultiplier * damage));
    }
}
