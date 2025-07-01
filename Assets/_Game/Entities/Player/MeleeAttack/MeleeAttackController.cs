using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using F3PS;

public class MeleeAttackController : MonoBehaviour
{
    private MeleeAttackData meleeAttackData => GameManager.Instance.PlayerData.MeleeAttackData;

    [Space(10)]
    [Header("Attack Settings")]
    public float recoilPower;

    [Space(10)]
    [Header("References")]
    public Transform userSpace;
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public GameObject muzzle;
    public ObjectPool projectilePool;
    public StaminaManager staminaManager;
    public CinemachineImpulseSource screenShakeSource;
    public Animator animator;
    public Collider[] ownColliders;

    [Space(10)]
    [Header("HUD")]
    public MeleeAttackHUD hud;

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
            var meleeProjectile = projectile.GetComponent<MeleeProjectile>();
            meleeProjectile.Init(userSpace.GetInstanceID(), ownColliders);
            projectile.SetActive(false);
        }
    }

    protected IEnumerator Shoot(Vector3 targetPosition)
    {
        var attackSpeed = meleeAttackData.AttackSpeed;
        var spreadAngle = meleeAttackData.SpreadAngle;
        var attackCoolDownTimer = meleeAttackData.AttackCoolDownTimer;

        isAttacking = true;
        attackCoolDownTime = attackCoolDownTimer;

        muzzle.SetActive(true);
        for (int i = 0; i < meleeAttackData.NumberOfProjectiles; i++)
        {
            float xRotation = Random.Range(-spreadAngle, spreadAngle);
            float yRotation = Random.Range(-spreadAngle, spreadAngle);
            Quaternion projectileOrientation = Quaternion.Euler(xRotation, yRotation, 0f) * projectileSpawn.rotation;
            var targetDirection = projectileOrientation * Vector3.forward * Vector3.Magnitude(targetPosition - projectileSpawn.position);
            var projectileObject = projectilePool.GetObject();
            var projectileTransform = projectileObject.transform;
            projectileTransform.position = projectileSpawn.position;
            projectileTransform.rotation = projectileOrientation;
            var meleeProjectile = projectileObject.GetComponent<MeleeProjectile>();
            projectileObject.SetActive(true);
            meleeProjectile.Shoot(attackSpeed);
        }
        var shootDirection = (targetPosition - projectileSpawn.position).normalized;
        screenShakeSource.GenerateImpulseWithVelocity(-shootDirection * recoilPower);
        while (attackCoolDownTime > 0f)
        {
            attackCoolDownTime -= Time.deltaTime;
            hud.UpdateCoolDown(1f - attackCoolDownTime / attackCoolDownTimer);
            yield return null;
        }

        muzzle.SetActive(false);
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
            if (staminaManager.IsRecoveringStamina)
            {
                hud.OnTryAttackWithoutStamina();
            }
            else
            {
                animator.SetTrigger("MeleeAttack");
                StartCoroutine(Shoot(targetPosition));
                staminaManager.Deplete(meleeAttackData.StaminaCost);
            }
            wasAttackingPressedLastFrame = true;
        }
        else if (stopAttacking)
        {
            wasAttackingPressedLastFrame = false;
        }
    }
}
