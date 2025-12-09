using UnityEngine;

public class YaggiStandardHittable : Hittable
{
    public YaggiStandardController yaggi;

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
        var finalDamage = Mathf.RoundToInt(damage * damageMultiplier);
        Debug.Log($"Yaggi hit with {finalDamage} damage from {hitDirection}");
        yaggi.Hit(finalDamage);
    }
}
