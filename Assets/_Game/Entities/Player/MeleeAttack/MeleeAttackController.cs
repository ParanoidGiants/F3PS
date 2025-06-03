using Cinemachine;
using DarkTonic.MasterAudio;
using System.Collections;
using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    [Space(10)]
    [Header("Attack Settings")]
    public int numberOfProjectiles;
    public float spreadAngle;
    public float attackSpeed = 100f;
    public float attackCoolDownTimer = 0.2f;
    public float recoilPower;
    public float staminaCost = 10f;

    [Space(10)]
    [Header("References")]
    public Transform origin;
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public ObjectPool projectilePool;
    public StaminaManager staminaManager;
    public CinemachineImpulseSource screenShakeSource;
    public SelectAttackControllerHUD attackControllerHUD;
    public MeleeAttackHud hud;
    public Collider[] collidersToIgnore;

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

    protected IEnumerator Shoot(Vector3 targetPosition)
    {
        isAttacking = true;
        attackCoolDownTime = attackCoolDownTimer;
            
        for (int i = 0; i < numberOfProjectiles; i++)
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
            meleeProjectile.Init(origin.GetInstanceID(), collidersToIgnore);
            meleeProjectile.BeforeSetActive(attackSpeed);
            projectileObject.SetActive(true);
        }
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
