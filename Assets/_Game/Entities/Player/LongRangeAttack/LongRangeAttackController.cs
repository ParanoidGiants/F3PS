using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LongRangeAttackController : MonoBehaviour
{
    [Space(10)]
    [Header("Attack Settings")]
    public int numberOfProjectiles;
    public float attackSpeed = 100f;
    public float impactForceMultiplier = 1.0f;
    public float attackCoolDownTimer = 0.2f;
    public float recoilPower;
    public float staminaCost = 10f;

    [Space(10)]
    [Header("References")]
    public Transform userSpace;
    public Transform projectileSpawn;
    public ObjectPool projectilePool;
    public StaminaManager staminaManager;
    public CinemachineImpulseSource screenShakeSource;
    public Collider[] ownColliders;

    [Space(10)]
    [Header("HUD")]
    public LongRangeAttackHUD hud;

    [Space(10)]
    [Header("Watchers")]
    public float attackCoolDownTime = 0.0f;
    public bool isAttackingPressedThisFrame;
    public bool wasAttackingPressedLastFrame;
    public bool isAttacking;

    public void Init()
    {
        projectilePool.Init(userSpace);
        var projectiles = projectilePool.GetObjects();
        foreach (var projectile in projectiles)
        {
            var longRangeProjectile = projectile.GetComponent<LongRangeProjectile>();
            longRangeProjectile.Init(userSpace.GetInstanceID(), ownColliders);
            projectile.SetActive(false);
        }
    }

    public void UpdateRotation(Quaternion rotation)
    {
        userSpace.rotation = rotation;
    }

    protected IEnumerator Shoot(Vector3 targetPosition)
    {
        isAttacking = true;
        attackCoolDownTime = attackCoolDownTimer;

        var targetDirection = projectileSpawn.rotation * (targetPosition - projectileSpawn.position).normalized;
        var projectileObject = projectilePool.GetObject();
        var projectileTransform = projectileObject.transform;
        projectileTransform.position = projectileSpawn.position;
        projectileTransform.rotation = projectileSpawn.rotation;
        var meleeProjectile = projectileObject.GetComponent<LongRangeProjectile>();
        projectileObject.SetActive(true);
        meleeProjectile.Shoot(attackSpeed, impactForceMultiplier);
        var shootDirection = (targetPosition - projectileSpawn.position).normalized;
        screenShakeSource.GenerateImpulseWithVelocity(-shootDirection * recoilPower);
        while (attackCoolDownTime > 0f)
        {
            attackCoolDownTime -= Time.deltaTime;
            hud.UpdateCoolDown(1f - attackCoolDownTime / attackCoolDownTimer);
            yield return null;
        }
        isAttacking = false;
    }

    public void OnUpdate(bool isAttackingPressed, Vector3 targetPosition)
    {
        wasAttackingPressedLastFrame = isAttackingPressedThisFrame;
        isAttackingPressedThisFrame = isAttackingPressed;

        var startAttacking = isAttackingPressedThisFrame && !wasAttackingPressedLastFrame;
        var stopAttacking = !isAttackingPressedThisFrame && wasAttackingPressedLastFrame;

        if (!isAttacking && startAttacking)
        {
            if (!staminaManager.HasEnoughStamina(staminaCost))
            {
                hud.OnTryAttackWithoutStamina();
            }
            else
            {
                StartCoroutine(Shoot(targetPosition));
                staminaManager.Deplete(staminaCost);
            }
            wasAttackingPressedLastFrame = true;
        }
        else if (stopAttacking)
        {
            wasAttackingPressedLastFrame = false;
        }
    }
}
