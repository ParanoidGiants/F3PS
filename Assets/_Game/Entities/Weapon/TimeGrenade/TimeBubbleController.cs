using UnityEngine;
    
public class TimeBubbleController : MonoBehaviour
{
    [Header("References")]
    public Transform userSpace;
    public SelectSkillControllerHUD selectSkillControllerHUD;
    public HittableManager hittableManager;
    public TimeBubbleGrenadeProjectile timeBubbleGrenadeProjectile;
    public LineRenderer throwLine;
    public Transform spawnTransform;
        
    [Space(10)]
    [Header("Settings")]
    public float throwPower;
    public int lineResolution = 100;
    public float lineStepSize = 0.1f;
    public LayerMask whatCanCollide;
    public bool isUnlocked;
    public float timeBubbleTimeScaleSpeed = 1f;

    [Space(10)]
    [Header("Watchers")]
    public bool isThrown;
    public bool isTimeBubbleActive;
    public bool isAimingThisFrame;
    public bool wasAimingLastFrame;
    public bool isDeactivated;
    public Vector3 throwDirection;

    private void Awake()
    {
        throwLine.positionCount = lineResolution;
        selectSkillControllerHUD = FindObjectOfType<SelectSkillControllerHUD>();
        timeBubbleGrenadeProjectile.Init(userSpace.GetInstanceID(), hittableManager);
        var projectileCollider = timeBubbleGrenadeProjectile.GetComponent<Collider>();
        foreach (var collider in hittableManager.colliders)
        {
            Physics.IgnoreCollision(projectileCollider, collider);
        }
    }

    private void OnEnable()
    {
        selectSkillControllerHUD.SelectTimeBubbleHud();
    }

    public void OnUpdate(bool isAiming, float bubbleTimeScaleChange, Vector3 targetPosition)
    {
        wasAimingLastFrame = isAimingThisFrame;
        isAimingThisFrame = isAiming;

        throwDirection = (targetPosition - spawnTransform.position).normalized;
        if (timeBubbleGrenadeProjectile.IsProjectileUpAndRunning)
        {
            if (isAimingThisFrame && !wasAimingLastFrame)
            {
                isThrown = false;
                isDeactivated = true;
                timeBubbleGrenadeProjectile.DeactivateTimeBubble();
            }
        }
        else if (!isThrown && !timeBubbleGrenadeProjectile.IsTimeBubbleActiveAndEnabled)
        {
            if (!isDeactivated)
            {
                if (isAimingThisFrame && wasAimingLastFrame)
                {
                    UpdateThrowLine();
                }
                else if (wasAimingLastFrame)
                {
                    ThrowGrenade();
                    HideThrowLine();
                    wasAimingLastFrame = false;
                }
                else if (isAimingThisFrame)
                {
                    UpdateThrowLine();
                    ShowThrowLine();
                    wasAimingLastFrame = true;
                }
            }
            else if (!isAimingThisFrame)
            {
                isDeactivated = false;
            }
        }

        if (bubbleTimeScaleChange != 0f)
        {
            timeBubbleGrenadeProjectile.PitchTimeScale(bubbleTimeScaleChange * timeBubbleTimeScaleSpeed);
        }
    }

    private void ThrowGrenade()
    {
        isThrown = true;
        timeBubbleGrenadeProjectile.gameObject.SetActive(false);
        timeBubbleGrenadeProjectile.BeforeSetActive(
            spawnTransform.position,
            spawnTransform.position + throwDirection,
            throwPower
        );
        timeBubbleGrenadeProjectile.gameObject.SetActive(true);
    }

    private void ShowThrowLine()
    {
        throwLine.enabled = true;
        throwLine.positionCount = lineResolution;
    }

    private void HideThrowLine()
    {
        throwLine.enabled = false;
    }

    private void UpdateThrowLine()
    {
        Vector3 spawnPosition = spawnTransform.position;
        float gravity = - timeBubbleGrenadeProjectile.Gravity;
        float throwAngleCos = Vector3.Dot(throwDirection, Vector3.up);
        float throwAngle = -Mathf.PI * 0.5f + Mathf.Acos(throwAngleCos);
            
        throwLine.positionCount = lineResolution;
        Vector3 lastPosition = default;

        for (int i = 0; i < lineResolution; i++)
        {
            float simulationTime = i * lineStepSize;
            float displacementZ = throwPower * Mathf.Cos(throwAngle) * simulationTime;
            float displacementY = -0.5f * gravity * simulationTime * simulationTime
                                    + throwPower * Mathf.Sin(throwAngle) * simulationTime;
            var displacement = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) 
                                * (new Vector3(0, -displacementY, displacementZ));
            var position = spawnPosition + displacement;
                
            if (i > 0)
            {
                var rayDirection = position - lastPosition;
                RaycastHit hit;
                Ray ray = new Ray(lastPosition, rayDirection);
                if (Physics.Raycast(ray, out hit, rayDirection.magnitude, whatCanCollide.value))
                {
                    throwLine.SetPosition(i, hit.point);
                    throwLine.positionCount = i + 1;
                    break;
                }
            }
            throwLine.SetPosition(i, position);
            lastPosition = position;
        }
    }

    public bool IsAiming()
    {
        return !timeBubbleGrenadeProjectile.IsProjectileUpAndRunning
            && !isThrown
            && !timeBubbleGrenadeProjectile.IsTimeBubbleActiveAndEnabled
            && !isDeactivated
            && isAimingThisFrame
            && wasAimingLastFrame;
    }
}

