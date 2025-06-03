using Cinemachine;
using System.Collections;
using UnityEngine;

public class LongRangeAttackController : MonoBehaviour
{
    [Space(10)]
    [Header("Attack Settings")]
    public int numberOfProjectiles;
    public float attackSpeed = 100f;
    public float attackCoolDownTimer = 0.2f;
    public float recoilPower;
    public float staminaCost = 10f;

    [Space(10)]
    [Header("References")]
    public Transform origin;
    public Transform projectileSpawn;
    public ObjectPool projectilePool;
    public StaminaManager staminaManager;
    public CinemachineImpulseSource screenShakeSource;
    public SelectAttackControllerHUD attackControllerHUD;
    public LongRangeAttackHUD hud;

    [Space(10)]
    [Header("Watchers")]
    public float attackCoolDownTime = 0.0f;
    public bool isAttackingPressedThisFrame;
    public bool wasAttackingPressedLastFrame;
    public bool isAttacking;

    private void OnEnable()
    {
        attackControllerHUD.SelectMeleeAttackHud();
    }

    public void Init()
    {
        projectilePool.Init(origin);
    }

    public void UpdateRotation(Quaternion rotation)
    {
        origin.rotation = rotation;
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
        var meleeProjectile = projectileObject.GetComponent<MeleeProjectile>();
        meleeProjectile.BeforeSetActive(attackSpeed);
        projectileObject.SetActive(true);
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
