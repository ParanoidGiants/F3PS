using F3PS.Damage.Take;
using StarterAssets;
using UnityEngine;

public class ScorpionHittable : Hittable
{
    private int _playerId;
    public ScorpionController scorpion;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _hittableId = scorpion.GetInstanceID();
        _playerId = FindFirstObjectByType<ThirdPersonController>().transform.parent.GetInstanceID();
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
    public void OnHit(HitBox hitBy, Vector3 hitDirection)
    {
        if (scorpion.isDead)
        {
            return;
        }
        scorpion.Hit((int)(damageMultiplier * hitBy.damage));
        if (hitBy.attackerId == _playerId && scorpion.currentState is not ScorpionState.AGGRESSIVE)
        {
            scorpion.HitByPlayerFrom(hitDirection);
        }
    }
}
