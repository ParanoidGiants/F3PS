using UnityEngine;

public class YaghotepHittable : Hittable
{
    public YaghotepController yaggi;

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
        if (yaggi.IsDead)
        {
            return;
        }
        yaggi.HitByPlayerFrom(hitDirection);
        yaggi.Hit((int)(damageMultiplier * damage));
    }
}
