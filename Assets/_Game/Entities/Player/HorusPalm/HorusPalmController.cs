using F3PS;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class HorusPalmController : MonoBehaviour
{
    private PlayerData PlayerData => GameManager.Instance.GameData.PlayerData;
    private PlayerEventController PlayerEventController => GameManager.Instance.saveGameManager.PlayerEventController;

    [Space(10)]
    [Header("Attack Settings")]
    public float recoilPower;

    [Space(10)]
    [Header("References")]
    public Transform userSpace;
    public Transform projectileSpawn;
    public ObjectPool projectilePool;
    public StaminaManager staminaManager;
    public CinemachineImpulseSource screenShakeSource;
    public Animator animator;
    public Collider[] ownColliders;

    [Space(10)]
    [Header("HUD")]
    public HorusPalmHUD hud;

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
            var horusPalmProjectile = projectile.GetComponent<HorusPalmProjectile>();
            horusPalmProjectile.Init(userSpace.gameObject, ownColliders);
            projectile.SetActive(false);
        }
    }

    public void UpdateRotation(Quaternion rotation)
    {
        userSpace.rotation = rotation;
    }

    protected IEnumerator Shoot(Vector3 targetPosition)
    {
        var attackCoolDownTimer = PlayerData.HorusPalmData.AttackCoolDownTimer;
        var attackSpeed = PlayerData.HorusPalmData.AttackSpeed;
        var impactForceMultiplier = PlayerData.HorusPalmData.ImpactForceMultiplier;

        isAttacking = true;
        attackCoolDownTime = attackCoolDownTimer;

        var targetDirection = projectileSpawn.rotation * (targetPosition - projectileSpawn.position).normalized;
        var projectileObject = projectilePool.GetObject();
        var projectileTransform = projectileObject.transform;
        projectileTransform.position = projectileSpawn.position;
        projectileTransform.rotation = projectileSpawn.rotation;
        var projectile = projectileObject.GetComponent<HorusPalmProjectile>();
        projectileObject.SetActive(true);
        projectile.Shoot(attackSpeed, impactForceMultiplier);
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
        var staminaCost = PlayerData.HorusPalmData.StaminaCost;
        wasAttackingPressedLastFrame = isAttackingPressedThisFrame;
        isAttackingPressedThisFrame = isAttackingPressed;

        var startAttacking = isAttackingPressedThisFrame && !wasAttackingPressedLastFrame;
        var stopAttacking = !isAttackingPressedThisFrame && wasAttackingPressedLastFrame;

        if (!isAttacking && startAttacking)
        {
            if (staminaManager.IsRecoveringStamina)
            {
                hud.OnTryAttackWithoutStamina();
            }
            else
            {
                animator.SetTrigger("HorusPalm");
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
