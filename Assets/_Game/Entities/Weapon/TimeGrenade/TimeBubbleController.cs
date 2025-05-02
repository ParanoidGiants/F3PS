using UnityEngine;
    
public class TimeBubbleController : MonoBehaviour
{
    [Header("References")]
    public Transform userSpace;
    public TimeBubbleHUD hud;
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
        
    [Space(10)]
    [Header("Watchers")]
    public bool isTimeBubbleActive;
    public bool wasAimingLastFrame;
    public Vector3 throwDirection;

    private void Awake()
    {
        throwLine.positionCount = lineResolution;

        timeBubbleGrenadeProjectile.Init(userSpace.GetInstanceID(), hittableManager);
        var projectileCollider = timeBubbleGrenadeProjectile.GetComponent<Collider>();
        foreach (var collider in hittableManager.colliders)
        {
            Physics.IgnoreCollision(projectileCollider, collider);
        }
    }

    private void OnEnable()
    {
        hud.gameObject.SetActive(true);
    }
        
    private void OnDisable()
    {
        hud.gameObject.SetActive(false);
    }

    public void OnUpdate(bool isAiming, Vector3 targetPosition)
    {
        throwDirection = (targetPosition - spawnTransform.position).normalized;
        hud.UpdateGrenadeEffect(timeBubbleGrenadeProjectile.LifeTimePercentage);


        if (isAiming && wasAimingLastFrame)
        {
            UpdateThrowLine();
        }
        else if (wasAimingLastFrame)
        {
            ThrowGrenade();
            HideThrowLine();
            wasAimingLastFrame = false;
        }
        else if (isAiming)
        {
            UpdateThrowLine();
            ShowThrowLine();
            wasAimingLastFrame = true;
        }
    }

    private void ThrowGrenade()
    {
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
}

